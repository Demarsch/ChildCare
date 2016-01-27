using System.Collections.Generic;
using System.Windows.Controls;

namespace AdminModule.Model
{
    public interface IDatabaseValidator
    {
        IEnumerable<ValidationResult> Validate();
    }
}
