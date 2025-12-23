using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

/// <summary>
/// Wraps a value and tracks whether it has been explicitly changed since initialisation or the last reset.
/// 
/// Useful for scenarios where updates should only be applied or propagated when a value
/// has been modified (e.g. partial updates, patch operations, or change tracking).
/// </summary>
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
