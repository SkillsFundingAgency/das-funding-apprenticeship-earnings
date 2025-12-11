using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

public class TrackedValue<T>
{
    private T _value;
    private bool _hasChanged = false;

    internal T Value => _value;
    public bool HasChanged => _hasChanged;

    public void SetValue(T value)
    {
        _hasChanged = true;
        _value = value;
    }

    public void SetFromTrackedValue(TrackedValue<T> trackedValue)
    {
        if (trackedValue.HasChanged)
            SetValue(trackedValue.Value);
    }

    public void ResetValue(T value)
    {
        _hasChanged = false;
        _value = value;
    }

    public TrackedValue(T initialValue)
    {
        _value = initialValue;
    }
}
