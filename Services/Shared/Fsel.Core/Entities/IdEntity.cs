using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Fsel.Core.Base.Interfaces;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Fsel.Core.Entities
{
    public class IdEntity : IdentityUser, IValidationEntity, IEntity
    {
        [Column(Order = 101)]
        public Guid CreatedUserId { get; set; }

        [Column(Order = 102)]
        public Guid? UpdatedUserId { get; set; }

        [Column(Order = 103)]
        public Guid? DeletedUserId { get; set; }

        [Column(Order = 104)]
        [MaxLength(100)]
        public string CreatedUserName { get; set; }

        [Column(Order = 105)]
        [MaxLength(100)]
        public string? UpdatedUserName { get; set; }

        [Column(Order = 106)]
        [MaxLength(100)]
        public string? DeletedUserName { get; set; }

        [Column(Order = 107)]
        public DateTime CreatedDate { get; set; }

        [Column(Order = 108)]
        public DateTime? UpdatedDate { get; set; }

        [Column(Order = 109)]
        public DateTime? DeletedDate { get; set; }

        [Column(Order = 110)]
        [DefaultValue("false")]
        public bool IsDeleted { get; set; }

        private int? _requestedHashCode;

        private List<INotification>? _domainEvents;

        public IReadOnlyCollection<INotification> DomainEvents {
            get
            {
                if (_domainEvents != null)
                    return _domainEvents.AsReadOnly();
                return new List<INotification>();
            }
        } 

        protected IdEntity()
        {
            CreatedDate = DateTime.UtcNow;
            CreatedUserId = Guid.Empty;
            CreatedUserName = string.Empty;
        }

        public void AddDomainEvent(INotification eventItem)
        {
            _domainEvents = _domainEvents ?? new List<INotification>();
            _domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(INotification eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

        public bool IsTransient()
        {
            return Id == Guid.Empty.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is Entity))
            {
                return false;
            }

            if (this == obj)
            {
                return true;
            }

            if (GetType() != obj.GetType())
            {
                return false;
            }

            IdEntity entity = (IdEntity)obj;
            if (entity.IsTransient() || IsTransient())
            {
                return false;
            }

            return entity.Id == Id;
        }

        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                if (!_requestedHashCode.HasValue)
                {
                    _requestedHashCode = Id.GetHashCode() ^ 0x1F;
                }

                return _requestedHashCode.Value;
            }

            return base.GetHashCode();
        }

        #region Validation
        protected List<ErrorResult> _errorMessages = new List<ErrorResult>();

        [JsonIgnore]
        public IReadOnlyCollection<ErrorResult> ErrorMessages => _errorMessages;

        public Assembly GetAssembly()
        {
            return GetType().Assembly;
        }

        public void AddValidationError(string errorCode, string propertyName, object propertyValue)
        {
            AddValidationError(errorCode, new List<string> { MethodHelper.GenerateErrorResult(propertyName, propertyValue) });
        }

        public void AddValidationError(string errorCode, List<string> errorValues)
        {
            _errorMessages.Add(new ErrorResult
            {
                ErrorCode = errorCode,
                ErrorMessage = MethodHelper.GetErrorMessage(errorCode, GetAssembly()),
                ErrorValues = errorValues
            });
        }

        public void AddValidationErrors(IEnumerable<ErrorResult> errorMessages)
        {
            _errorMessages.AddRange(errorMessages);
        }

        public virtual bool IsValid()
        {
            ValidationContext validationContext = new ValidationContext(this, null, null);
            List<ValidationResult> list = new List<ValidationResult>();
            if (!Validator.TryValidateObject(this, validationContext, list, validateAllProperties: true))
            {
                foreach (ValidationResult item in list)
                {
                    ErrorResult errorResult = new ErrorResult
                    {
                        ErrorCode = item.ErrorMessage
                    };
                    errorResult.ErrorMessage = MethodHelper.GetErrorMessage(item.ErrorMessage, GetAssembly());
                    foreach (string memberName in item.MemberNames)
                    {
                        PropertyInfo? property = validationContext.ObjectType.GetProperty(memberName);
                        object? value = property?.GetValue(validationContext.ObjectInstance, null);
                        errorResult.ErrorValues.Add(MethodHelper.GenerateErrorResult(memberName, value));
                    }

                    _errorMessages.Add(errorResult);
                }
            }

            return _errorMessages.Count == 0;
        }
        #endregion
    }
}
