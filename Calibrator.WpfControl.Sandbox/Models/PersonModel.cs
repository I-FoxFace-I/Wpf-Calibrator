using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Calibrator.WpfControl.Controls.ScSmartTable;
using Calibrator.WpfControl.Controls.ScSmartTable.Models;
using Calibrator.WpfControl.Controls.UniTable;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Sandbox.Models;

// Model for demo purposes
public class PersonModel : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private int _age;
    private string _email = string.Empty;
    private string _city = string.Empty;
    private bool _isActive;
    private decimal _salary;
    private DateTime _hireDate;
    private string _department = string.Empty;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public int Age
    {
        get => _age;
        set { _age = value; OnPropertyChanged(); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    public string City
    {
        get => _city;
        set { _city = value; OnPropertyChanged(); }
    }

    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; OnPropertyChanged(); }
    }

    public decimal Salary
    {
        get => _salary;
        set { _salary = value; OnPropertyChanged(); }
    }

    public DateTime HireDate
    {
        get => _hireDate;
        set { _hireDate = value; OnPropertyChanged(); }
    }

    public string Department
    {
        get => _department;
        set { _department = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
