using Core.Data;
using Core.Data.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommissionsModule.ViewModels
{
    public class CommissionСonclusionViewModel
    {
        #region Methods
        public async void Initialize(int commissionProtocolId = SpecialValues.NonExistingId, int personId = SpecialValues.NonExistingId)
        {
            //CommissionProtocolId = commissionProtocolId;
            //SelectedCommissionTypeId = -1;
            //SelectedCommissionQuestionId = -1;
            //SelectedCommissionSourceId = -1;
            //MKB = string.Empty;
            //SelectedSentLPUId = -1;
            //SelectedTalonId = -1;
            //SelectedHelpTypeId = -1;
            //IncomeDateTime = DateTime.Now;
            //if (currentOperationToken != null)
            //{
            //    currentOperationToken.Cancel();
            //    currentOperationToken.Dispose();
            //}
            //var loadingIsCompleted = false;
            //currentOperationToken = new CancellationTokenSource();
            //var token = currentOperationToken.Token;
            //BusyMediator.Activate("Загрузка протокола комиссии...");
            //logService.InfoFormat("Loading commission protocol with id ={0}", commissionProtocolId);
            //var commissionProtocolQuery = commissionService.GetCommissionProtocolById(commissionProtocolId);
            //var curDate = DateTime.Now;
            //var persId = personId;
            //try
            //{
            //    var commissionProtocolData = await commissionProtocolQuery.Select(x => new
            //    {
            //        x.CommissionTypeId,
            //        x.CommissionQuestionId,
            //        x.CommissionSourceId,
            //        x.MKB,
            //        x.SentLPUId,
            //        x.PersonTalonId,
            //        x.MedicalHelpTypeId,
            //        x.IncomeDateTime,
            //        x.PersonId
            //    }).FirstOrDefaultAsync(token);
            //    if (commissionProtocolData != null)
            //    {
            //        persId = commissionProtocolData.PersonId;
            //        curDate = commissionProtocolData.IncomeDateTime;
            //    }
            //    var res = await LoadDataSources(curDate, persId);
            //    if (!res)
            //    {
            //        logService.ErrorFormatEx(null, "Failed to load commission data sources with commission id ={0}", commissionProtocolId);
            //        FailureMediator.Activate("Не удалость загрузить справочники. Попробуйте еще раз или обратитесь в службу поддержки", reInitializeCommandWrapper);
            //        return;
            //    }

            //    if (commissionProtocolData != null)
            //    {
            //        SelectedCommissionTypeId = commissionProtocolData.CommissionTypeId;
            //        SelectedCommissionQuestionId = commissionProtocolData.CommissionQuestionId;
            //        SelectedCommissionSourceId = commissionProtocolData.CommissionSourceId;
            //        MKB = commissionProtocolData.MKB;
            //        SelectedSentLPUId = commissionProtocolData.SentLPUId;
            //        SelectedTalonId = commissionProtocolData.PersonTalonId.ToInt();
            //        SelectedHelpTypeId = commissionProtocolData.MedicalHelpTypeId.ToInt();
            //        IncomeDateTime = commissionProtocolData.IncomeDateTime;
            //    }
            //    loadingIsCompleted = true;
            //}
            //catch (OperationCanceledException)
            //{
            //    //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            //}
            //catch (Exception ex)
            //{
            //    logService.ErrorFormatEx(ex, "Failed to load commission protocol with id ={0}", commissionProtocolId);
            //    FailureMediator.Activate("Не удалость загрузить протокол комиссии. Попробуйте еще раз или обратитесь в службу поддержки", reInitializeCommandWrapper, ex);
            //    loadingIsCompleted = true;
            //}
            //finally
            //{
            //    if (loadingIsCompleted)
            //        BusyMediator.Deactivate();
            //    if (commissionProtocolQuery != null)
            //        commissionProtocolQuery.Dispose();
            //}
        }

        public void GetСonclusionCommissionProtocolData(ref CommissionProtocol commissionProtocol)
        {
            
        }
        #endregion
    }
}
