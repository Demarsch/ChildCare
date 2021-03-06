﻿using System;
using Core;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class PatientAssignmentViewModel : ObservableObject
    {
        private readonly AssignmentScheduleDTO assignment;

        private readonly ICacheService cacheService;

        public PatientAssignmentViewModel(AssignmentScheduleDTO assignment, ICacheService cacheService)
        {
            if (assignment == null)
                throw new ArgumentNullException("assignment");
            if (cacheService == null)
                throw new ArgumentNullException("cacheService");
            this.cacheService = cacheService;
            this.assignment = assignment;
        }

        public DateTime AssignDateTime { get { return assignment.AssignDateTime; } }

        public bool IsCancelled { get { return assignment.IsCanceled; } }

        public bool IsCompleted { get { return assignment.IsCompleted; } }

        public bool IsTemporary { get { return assignment.IsTemporary; } }

        public string Room
        {
            get
            {
                var room = cacheService.GetItemById<Room>(assignment.RoomId);
                return room == null ? "Неизвестный кабинет" : room.Name;
            }
        }

        public string RecordType
        {
            get
            {
                var recordType = cacheService.GetItemById<RecordType>(assignment.RecordTypeId);
                return recordType == null ? "Неизвестное назначение" : recordType.Name;
            }
        }

        public AssignmentState State
        {
            get
            {
                return IsCancelled
                    ? AssignmentState.Cancelled
                    : IsCompleted 
                        ?  AssignmentState.Completed 
                        : IsTemporary ? AssignmentState.Temporary : AssignmentState.Incompleted;
            }
        }
    }

    public enum AssignmentState
    {
        Incompleted,
        Completed,
        Cancelled,
        Temporary
    }
}