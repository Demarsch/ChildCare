﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Core.Data;
using Prism.Mvvm;

namespace Shared.Patient.ViewModels
{
    public class FoundPersonViewModel : BindableBase
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private IEnumerable<string> wordsToHighlight;

        public IEnumerable<string> WordsToHighlight
        {
            get { return wordsToHighlight; }
            set { SetProperty(ref wordsToHighlight, value); }
        }

        private DateTime birthDate;

        public DateTime BirthDate
        {
            get { return birthDate; }
            set { SetProperty(ref birthDate, value); }
        }

        private bool isMale;

        public bool IsMale
        {
            get { return isMale; }
            set { SetProperty(ref isMale, value); }
        }

        private string snils;

        public string Snils
        {
            get { return snils; }
            set
            {
                value = string.IsNullOrWhiteSpace(value) ? Person.UnknownSnils : value;
                SetProperty(ref snils, value);
            }
        }

        private string medNumber;

        public string MedNumber
        {
            get { return medNumber; }
            set
            {
                value = string.IsNullOrWhiteSpace(value) ? Person.UnknownMedNumber : value;
                SetProperty(ref medNumber, value);
            }
        }

        private PersonName currentName;

        public PersonName CurrentName
        {
            get { return currentName; }
            set { SetProperty(ref currentName, value); }
        }

        private PersonName previousName;

        public PersonName PreviousName
        {
            get { return previousName; }
            set { SetProperty(ref previousName, value); }
        }

        private PersonIdentityDocument identityDocument;

        public PersonIdentityDocument IdentityDocument
        {
            get { return identityDocument; }
            set { SetProperty(ref identityDocument, value); }
        }

        private ImageSource photo;

        public ImageSource Photo
        {
            get { return photo; }
            set { SetProperty(ref photo, value); }
        }

        public string FullName
        {
            get
            {
                var result = new StringBuilder();
                var hasCurrent = CurrentName != null && !string.IsNullOrWhiteSpace(CurrentName.LastName);
                var hasPrevious = PreviousName != null && !string.IsNullOrWhiteSpace(PreviousName.LastName);
                var theyAreDifferent = hasCurrent != hasPrevious || (hasCurrent && CurrentName.LastName != PreviousName.LastName);
                if (hasCurrent && hasPrevious && theyAreDifferent)
                {
                    result.Append(CurrentName.LastName)
                        .Append('(')
                        .Append(PreviousName.LastName)
                        .Append(')');
                }
                else if (hasCurrent)
                {
                    result.Append(CurrentName.LastName);
                }
                else if (hasPrevious)
                {
                    result.Append(PreviousName.LastName);
                }
                else
                {
                    result.Append(PersonName.UnknownLastName);
                }
                result.Append(' ');

                hasCurrent = CurrentName != null && !string.IsNullOrWhiteSpace(CurrentName.FirstName);
                hasPrevious = PreviousName != null && !string.IsNullOrWhiteSpace(PreviousName.FirstName);
                theyAreDifferent = hasCurrent != hasPrevious || (hasCurrent && CurrentName.FirstName != PreviousName.FirstName);
                if (hasCurrent && hasPrevious && theyAreDifferent)
                {
                    result.Append(CurrentName.FirstName)
                        .Append('(')
                        .Append(PreviousName.FirstName)
                        .Append(')');
                }
                else if (hasCurrent)
                {
                    result.Append(CurrentName.FirstName);
                }
                else if (hasPrevious)
                {
                    result.Append(PreviousName.FirstName);
                }
                else
                {
                    result.Append(PersonName.UnknownFirstName);
                }
                result.Append(' ');

                hasCurrent = CurrentName != null && !string.IsNullOrWhiteSpace(CurrentName.MiddleName);
                hasPrevious = PreviousName != null && !string.IsNullOrWhiteSpace(PreviousName.MiddleName);
                theyAreDifferent = hasCurrent != hasPrevious || (hasCurrent && CurrentName.MiddleName != PreviousName.MiddleName);
                if (hasCurrent && hasPrevious && theyAreDifferent)
                {
                    result.Append(CurrentName.MiddleName)
                        .Append('(')
                        .Append(PreviousName.MiddleName)
                        .Append(')');
                }
                else if (hasCurrent)
                {
                    result.Append(CurrentName.MiddleName);
                }
                else if (hasPrevious)
                {
                    result.Append(PreviousName.MiddleName);
                }
                if (result[result.Length - 1] == ' ')
                {
                    result.Remove(result.Length - 1, 1);
                }
                return result.ToString();
            }
        }
    }
}