using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Calibrator.WpfControl.Demo.Views;

public partial class DropdownDemo : UserControl
{
    public DropdownDemo()
    {
        InitializeComponent();
        DataContext = new DropdownDemoViewModel();
    }
}

public class DropdownDemoViewModel : INotifyPropertyChanged
{
    private string _selectedCountry;
    private City _selectedCity;
    private string _selectedStatus;

    public DropdownDemoViewModel()
    {
        Countries = new List<string>
        {
            "United States",
            "United Kingdom",
            "Germany",
            "France",
            "Japan",
            "Czech Republic"
        };

        Cities = new List<City>
        {
            new City { Id = 1, Name = "Prague" },
            new City { Id = 2, Name = "New York" },
            new City { Id = 3, Name = "London" },
            new City { Id = 4, Name = "Tokyo" },
            new City { Id = 5, Name = "Paris" }
        };

        Statuses = new List<string>
        {
            "Active",
            "Inactive",
            "Pending"
        };

        // Set default values
        SelectedCountry = Countries[5]; // Czech Republic
        SelectedCity = Cities[0]; // Prague
        SelectedStatus = Statuses[0]; // Active
    }

    public List<string> Countries { get; set; }
    public List<City> Cities { get; set; }
    public List<string> Statuses { get; set; }

    public string SelectedCountry
    {
        get => _selectedCountry;
        set
        {
            _selectedCountry = value;
            OnPropertyChanged();
        }
    }

    public City SelectedCity
    {
        get => _selectedCity;
        set
        {
            _selectedCity = value;
            OnPropertyChanged();
        }
    }

    public string SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            _selectedStatus = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class City
{
    public int Id { get; set; }
    public string Name { get; set; }
}
