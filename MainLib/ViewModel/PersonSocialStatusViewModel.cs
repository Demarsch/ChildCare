using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Threading.Tasks;
using Core;

namespace MainLib
{

    public class PersonSocialStatusViewModel : ObservableObject
    {
        #region Fields

        private readonly PersonSocialStatus personSocialStatus;

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
                WithoutEndDate = personSocialStatus.EndDateTime.Date == DateTime.MaxValue.Date;
            }
            else
            {
                SocialStatusTypeId = 0;
                Office = string.Empty;
                Org = null;
                BeginDate = new DateTime(1900, 1, 1);
                EndDate = DateTime.MaxValue.Date;
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

        private DateTime endDate = DateTime.MaxValue.Date;
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
                EndDate = DateTime.MaxValue;
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
    }
}
