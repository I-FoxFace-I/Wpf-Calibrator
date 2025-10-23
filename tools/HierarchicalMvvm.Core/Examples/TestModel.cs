namespace HierarchicalMvvm.Core.Examples
{
    /// <summary>
    /// Příklad jednoduchého modelu pro testování
    /// </summary>
    public class TestPersonModel : TrackableObject
    {
        private string _name = string.Empty;
        private int _age;
        private TestAddressModel? _address;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int Age
        {
            get => _age;
            set => SetProperty(ref _age, value);
        }

        public TestAddressModel? Address
        {
            get => _address;
            set => SetChildProperty(ref _address, value);
        }
    }

    /// <summary>
    /// Příklad child modelu
    /// </summary>
    public class TestAddressModel : TrackableObject
    {
        private string _street = string.Empty;
        private string _city = string.Empty;

        public string Street
        {
            get => _street;
            set => SetProperty(ref _street, value);
        }

        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }
    }

    /// <summary>
    /// Příklad root modelu s kolekcí
    /// </summary>
    public class TestCompanyModel : TrackableObject
    {
        private string _name = string.Empty;
        private TrackableCollection<TestPersonModel> _employees = new();

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public TrackableCollection<TestPersonModel> Employees
        {
            get => _employees;
            set => SetChildProperty(ref _employees, value);
        }

        public TestCompanyModel()
        {
            // Nastavit parent pro kolekci
            _employees.SetParent(this);
        }
    }
}