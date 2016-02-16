using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AdminModule.Exceptions;
using AdminModule.Model;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Services;
using Shared.Patient.Services;

namespace AdminModule.Services
{
    public class UserAccessService : IUserAccessService
    {
        private readonly ICacheService cacheService;

        private readonly IDbContextProvider contextProvider;

        public UserAccessService(ICacheService cacheService, IDbContextProvider contextProvider)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.cacheService = cacheService;
            this.contextProvider = contextProvider;
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.Factory.StartNew(() => cacheService.GetItems<Permission>());
        }

        public Task<IEnumerable<PermissionGroup>> GetGroupsAsync()
        {
            return Task.Factory.StartNew(() => cacheService.GetItems<PermissionGroup>());
        }

        public async Task<IEnumerable<UserDTO>> GetUsersAsync(int? userId = null)
        {
            using (var context = contextProvider.CreateLightweightContext())
            {
                return await context.Set<User>()
                                    .Where(x => userId == null || userId.Value == x.Id)
                                    .Select(x => new UserDTO
                                                 {
                                                     ActiveFrom = x.BeginDateTime,
                                                     ActiveTo = x.EndDateTime,
                                                     FullName = x.Person.FullName,
                                                     PersonId = x.PersonId,
                                                     PhotoData = x.Person.Document.FileData,
                                                     Sid = x.SID,
                                                     Login = x.Login,
                                                     Id = x.Id
                                                 })
                                    .ToArrayAsync();
            }
        }

        public async Task AddUserToGroupAsync(int userId, int groupId)
        {
            var group = cacheService.GetItemById<PermissionGroup>(groupId);
            if (group.UserPermissionGroups.Any(x => x.UserId == userId))
            {
                return;
            }
            var userGroupMembership = new UserPermissionGroup { PermissionGroupId = groupId, UserId = userId };
            await cacheService.AddItemAsync(userGroupMembership);
        }

        public async Task RemoveUserFromGroupAsync(int userId, int groupId)
        {
            var group = cacheService.GetItemById<PermissionGroup>(groupId);
            var userGroupMembership = group.UserPermissionGroups.FirstOrDefault(x => x.UserId == userId);
            if (userGroupMembership == null)
            {
                return;
            }
            await cacheService.RemoveItemAsync(userGroupMembership);
        }

        public async Task AddPermissionToGroupAsync(int permissionId, int groupId)
        {
            var group = cacheService.GetItemById<PermissionGroup>(groupId);
            if (group.PermissionGroupMemberships.Any(x => x.PermissionId == permissionId))
            {
                return;
            }
            var permissionGroupMembership = new PermissionGroupMembership { GroupId = groupId, PermissionId = permissionId };
            await cacheService.AddItemAsync(permissionGroupMembership);
        }

        public async Task RemovePermissionFromGroupAsync(int permissionId, int groupId)
        {
            var group = cacheService.GetItemById<PermissionGroup>(groupId);
            var permissionGroupMembership = group.PermissionGroupMemberships.FirstOrDefault(x => x.PermissionId == permissionId);
            if (permissionGroupMembership == null)
            {
                return;
            }
            await cacheService.RemoveItemAsync(permissionGroupMembership);
        }

        public async Task<PermissionGroup> CreateNewPermissionGroupAsync(string name, string description)
        {
            name = name.Trim();
            var groupWithTheSameName = cacheService.GetItemByName<PermissionGroup>(name);
            if (groupWithTheSameName != null)
            {
                throw new DataException("Group with the same name already exists");
            }
            var result = new PermissionGroup
                         {
                             Name = name,
                             Description = description,
                             PermissionGroupMemberships = new HashSet<PermissionGroupMembership>(),
                             UserPermissionGroups = new HashSet<UserPermissionGroup>(),
                         };
            await Task.Factory.StartNew(() => cacheService.AddItem(result));
            return result;
        }

        public async Task<PermissionGroup> SavePermissionGroupAsync(string newName, string newDescription, PermissionGroup currentGroup)
        {
            newName = newName.Trim();
            newDescription = (newDescription ?? string.Empty).Trim();
            var groupWithTheSameName = cacheService.GetItemByName<PermissionGroup>(newName);
            if (groupWithTheSameName != null && groupWithTheSameName.Id != currentGroup.Id)
            {
                throw new DataException("Group with the same name already exists");
            }
            currentGroup.Name = newName;
            currentGroup.Description = newDescription;
            await cacheService.UpdateItemAsync(currentGroup);
            return currentGroup;
        }

        public async Task DeletePermissionGroupAsync(PermissionGroup group)
        {
            await cacheService.RemoveItemAsync(group);
        }

        public async Task ChangeUserActivationAsync(User user)
        {
            user.EndDateTime = user.EndDateTime == SpecialValues.MaxDate ? DateTime.Today.AddDays(-1.0) : SpecialValues.MaxDate;
            await cacheService.UpdateItemAsync(user);
        }

        public async Task<Person> GetPersonAsync(int personId)
        {
            using (var context = contextProvider.CreateLightweightContext())
            {
                var query = context.Set<Person>().Where(x => x.Id == personId)
                                   .Include(x => x.PersonNames)
                                   .Include(x => x.Users)
                                   .Include(x => x.Document);
                return await query.FirstOrDefaultAsync();
            }
        }

        public async Task<User> SaveUserAsync(SaveUserInput input)
        {
            using (var context = contextProvider.CreateLightweightContext())
            {
                if (input.UserInfo.HasValue)
                {
                    if (cacheService.GetItems<User>().Any(x => x.SID == input.UserInfo.Value.Sid && x.PersonId != input.PersonId))
                    {
                        throw new UserWithSameSidExistsException();
                    }
                }
                User user;
                Person person;
                if (input.PersonId.IsNewOrNonExisting())
                {
                    person = new Person
                    {
                        Snils = input.Snils,
                        MedNumber = input.MedNumber,
                        Phones = string.Empty,
                        Email = string.Empty,
                        AmbNumberString = string.Empty
                    };
                    context.Entry(person).State = EntityState.Added;
                    var newName = new PersonName
                    {
                        BeginDateTime = SpecialValues.MinDate,
                        EndDateTime = SpecialValues.MaxDate,
                        FirstName = input.FirstName,
                        LastName = input.LastName,
                        MiddleName = input.MiddleName,
                        Person = person
                    };
                    context.Entry(newName).State = EntityState.Added;
                    person.FullName = newName.FullName;
                    person.ShortName = newName.ShortName;
                    user = new User
                    {
                        BeginDateTime = SpecialValues.MinDate,
                        EndDateTime = SpecialValues.MaxDate,
                        Login = input.UserInfo.Value.Login,
                        SID = input.UserInfo.Value.Sid,
                        SystemName = input.UserInfo.Value.FullName
                    };
                }
                else
                {
                    person = await context.Set<Person>()
                        .Include(x => x.PersonNames)
                        .Include(x => x.Users)
                        .Where(x => x.Id == input.PersonId).FirstAsync();
                    if (input.FirstName != null || input.MiddleName != null || input.LastName != null)
                    {
                        var oldName = person.PersonNames.First(x => x.EndDateTime == SpecialValues.MaxDate);
                        //Old name must be changed
                        PersonName currentName;
                        if (input.NewNameStartDate == null)
                        {
                            oldName.LastName = input.LastName ?? oldName.LastName;
                            oldName.FirstName = input.FirstName ?? oldName.FirstName;
                            oldName.MiddleName = input.MiddleName ?? oldName.MiddleName;
                            currentName = oldName;
                        }
                        else
                        {
                            oldName.EndDateTime = input.NewNameStartDate.Value.Date.AddDays(-1.0);
                            var newName = new PersonName
                            {
                                BeginDateTime = input.NewNameStartDate.Value.Date,
                                EndDateTime = SpecialValues.MaxDate,
                                FirstName = input.FirstName ?? oldName.FirstName,
                                LastName = input.LastName ?? oldName.LastName,
                                MiddleName = input.MiddleName ?? oldName.MiddleName,
                                PersonId = person.Id
                            };
                            context.Entry(newName).State = EntityState.Added;
                            currentName = newName;
                        }
                        person.ShortName = currentName.ShortName;
                        person.FullName = currentName.FullName;
                    }
                    if (input.UserInfo.HasValue)
                    {
                        var personUser = person.Users.FirstOrDefault();
                        if (personUser == null)
                        {
                            user = new User
                            {
                                BeginDateTime = SpecialValues.MinDate,
                                EndDateTime = SpecialValues.MaxDate,
                                PersonId = person.Id
                            };
                            context.Entry(person).State = EntityState.Added;
                        }
                        else
                        {
                            user = cacheService.GetItemById<User>(personUser.Id);
                        }
                        user.SID = input.UserInfo.Value.Sid;
                        user.Login = input.UserInfo.Value.Login;
                        user.SystemName = input.UserInfo.Value.FullName;
                    }
                    else
                    {
                        user = cacheService.GetItemById<User>(person.Users.First().Id);
                    }
                }
                person.BirthDate = input.BirthDate ?? person.BirthDate;
                person.IsMale = input.IsMale ?? person.IsMale;
                person.Snils = (input.Snils ?? person.Snils).ToSafeString();
                person.MedNumber = (input.MedNumber ?? person.MedNumber).ToSafeString();
                if (input.Photo.HasValue)
                {
                    //Delete old photo
                    if (person.PhotoId != null)
                    {
                        context.Entry(new Document { Id = person.PhotoId.Value }).State = EntityState.Deleted;
                    }
                    if (input.Photo.Value == null || input.Photo.Value.Length == 0)
                    {
                        person.PhotoId = null;
                    }
                    else
                    {
                        //Upload new photo
                        var newPhoto = new Document
                        {
                            FileData = input.Photo.Value,
                            Description = "фото",
                            DisplayName = "фото",
                            Extension = "jpg",
                            FileName = "фото",
                            FileSize = input.Photo.Value.Length,
                            UploadDate = DateTime.Now
                        };
                        person.Document = newPhoto;
                    }
                }
                context.ChangeTracker.DetectChanges();
                await context.SaveChangesAsync();
                if (user.PersonId == 0)
                {
                    user.PersonId = person.Id;
                    await cacheService.AddItemAsync(user);
                }
                else
                {
                    await cacheService.UpdateItemAsync(user);
                }
                return user;
            }
        }
    }
}