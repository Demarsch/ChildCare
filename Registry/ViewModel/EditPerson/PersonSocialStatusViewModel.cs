﻿using DataLib;
using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using Core;
using System.ComponentModel;

namespace Registry
{

    public class PersonSocialStatusViewModel : ObservableObject, IDataErrorInfo
    {
        #region Fields

        private PersonSocialStatus personSocialStatus;

        private IPersonService service;

        #endregion

        #region Constructors

        public PersonSocialStatusViewModel(PersonSocialStatus personSocialStatus, IPersonService service)
        {
            if (personSocialStatus == null)
                throw new ArgumentNullException("personSocialStatus");
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.personSocialStatus = personSocialStatus;
            SocialStatusOrgSuggestionProvider = new SocialStatusOrgSuggestionProvider(service);
            FillData();
        }

        #endregion

        #region Methods

        public PersonSocialStatus SetData()
        {
            if (personSocialStatus == null)
                personSocialStatus = new PersonSocialStatus();
            personSocialStatus.SocialStatusTypeId = SocialStatusTypeId;
            personSocialStatus.Office = Office;
            if (Org == null)
                personSocialStatus.Org = null;
            else
                personSocialStatus.OrgId = Org.Id;
            personSocialStatus.BeginDateTime = BeginDate;
            personSocialStatus.EndDateTime = endDate;

            return personSocialStatus;
        }

        private void FillData()
        {
            if (!IsEmpty)
            {
                SocialStatusTypeId = personSocialStatus.SocialStatusTypeId;
                Office = personSocialStatus.Office;
                if (personSocialStatus.OrgId.HasValue)
                    Org = service.GetOrg(personSocialStatus.OrgId.Value);
                else
                    Org = null;
                BeginDate = personSocialStatus.BeginDateTime;
                EndDate = personSocialStatus.EndDateTime;
                WithoutEndDate = personSocialStatus.EndDateTime == DateTime.MaxValue;
            }
            else
            {
                SocialStatusTypeId = 0;
                Office = string.Empty;
                Org = null;
                BeginDate = new DateTime(1900, 1, 1);
                EndDate = DateTime.MaxValue;
                WithoutEndDate = true;
            }
        }

        #endregion

        #region Properties

        private SocialStatusOrgSuggestionProvider socialStatusOrgSuggestionProvider = null;
        public SocialStatusOrgSuggestionProvider SocialStatusOrgSuggestionProvider
        {
            get { return socialStatusOrgSuggestionProvider; }
            set { Set(() => SocialStatusOrgSuggestionProvider, ref socialStatusOrgSuggestionProvider, value); }
        }

        public bool IsEmpty
        {
            get { return personSocialStatus == null; }
        }

        private int socialStatusTypeId = 0;
        public int SocialStatusTypeId
        {
            get { return socialStatusTypeId; }
            set
            {
                Set(() => SocialStatusTypeId, ref socialStatusTypeId, value);
                RaisePropertyChanged(() => NeedPlace);
            }
        }

        private string office = string.Empty;
        public string Office
        {
            get { return office; }
            set { Set(() => Office, ref office, value); }
        }

        private Org org = null;
        public Org Org
        {
            get { return org; }
            set { Set(() => Org, ref org, value); }
        }

        private DateTime beginDate = DateTime.MinValue;
        public DateTime BeginDate
        {
            get { return beginDate; }
            set
            {
                Set(() => BeginDate, ref beginDate, value);
                RaisePropertyChanged(() => PersonSocialStatusState);
                RaisePropertyChanged(() => PersonSocialStatusStateString);
            }
        }

        private DateTime endDate = DateTime.MaxValue;
        public DateTime EndDate
        {
            get { return endDate; }
            set
            {
                Set(() => EndDate, ref endDate, value);
                RaisePropertyChanged(() => PersonSocialStatusState);
                RaisePropertyChanged(() => PersonSocialStatusStateString);
            }
        }

        private bool withoutEndDate = false;
        public bool WithoutEndDate
        {
            get { return withoutEndDate; }
            set
            {
                Set(() => WithoutEndDate, ref withoutEndDate, value);
                if (withoutEndDate)
                    EndDate = DateTime.MaxValue;
                else
                    EndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);
                RaisePropertyChanged(() => WithEndDate);
            }
        }

        public bool WithEndDate
        {
            get { return !WithoutEndDate; }
        }

        public bool NeedPlace
        {
            get
            {
                var socialStatusType = service.GetSocialStatusType(SocialStatusTypeId);
                if (socialStatusType != null)
                    return socialStatusType.NeedPlace;
                return false;
            }
        }


        public ItemState PersonSocialStatusState
        {
            get
            {
                var datetimeNow = DateTime.Now;
                if (datetimeNow >= BeginDate && datetimeNow < EndDate)
                    return ItemState.Active;
                return ItemState.Inactive;
            }
        }

        public string PersonSocialStatusStateString
        {
            get
            {
                switch (PersonSocialStatusState)
                {
                    case ItemState.Active:
                        return "Действующий статус";
                    case ItemState.Inactive:
                        return "Недействующий статус";
                    default:
                        return string.Empty;
                }
            }
        }

        public string PersonSocialStatusesString
        {
            get
            {
                var socialStatusTypeName = string.Empty;
                var socialStatusType = service.GetSocialStatusType(SocialStatusTypeId);
                if (socialStatusType != null)
                    socialStatusTypeName = socialStatusType.Name;
                return socialStatusTypeName + (Org != null ? ": " + Org.Name + ", " + Office : string.Empty);
            }
        }

        #endregion

        #region Implementation IDataErrorInfo

        private bool saveWasRequested { get; set; }

        public readonly HashSet<string> invalidProperties = new HashSet<string>();

        public bool Invalidate()
        {
            saveWasRequested = true;
            RaisePropertyChanged(string.Empty);
            return invalidProperties.Count < 1;
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                if (columnName == "SocialStatusTypeId")
                {
                    result = SocialStatusTypeId < 1 ? "Укажите тип социального статуса" : string.Empty;
                }
                if (columnName == "Office")
                {
                    result = NeedPlace && string.IsNullOrEmpty(Office) ? "Укажите должность" : string.Empty;
                }
                if (columnName == "Org")
                {
                    result = NeedPlace && Org == null ? "Укажите нвзвание организации" : string.Empty;
                }
                if (columnName == "BeginDate" || columnName == "EndDate")
                {
                    result = BeginDate > EndDate ? "Дата начала не может быть больше даты окончания" : string.Empty;
                }
                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
