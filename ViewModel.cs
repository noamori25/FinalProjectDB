using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DBgenerator
{
    internal class ViewModel : DispatcherObject, INotifyPropertyChanged, IDataErrorInfo
    {
        private Random _random;
        public event PropertyChangedEventHandler PropertyChanged;
        public DelegateCommand AddToDB { get; set; }
        public DelegateCommand ReplaceDB { get; set; }
        private BackgroundWorker _backGround;
        private RandomDataGenerator _randomDataGenerator { get; set; }
        private bool _onWorking = false;




        public ViewModel()
        {
            _random = new Random();
            AddToDB = new DelegateCommand(ExecuteAdd, CanExecuteAdd);
            ReplaceDB = new DelegateCommand(ExecuteReplace, CanExecuteReplace);
            _backGround = new BackgroundWorker();
            _randomDataGenerator = new RandomDataGenerator();
            Message = _randomDataGenerator.Message;
            Status = 0;

        }
        private int numOfCustomers;
        public int numberOfCustomers
        {
            get
            {
                return numOfCustomers;
            }
            set
            {
                numOfCustomers = value;
                OnPropertyChanged("numberOfCustomers");
            }
        }
        private int numOfCountries;
        public int numberOfCountries
        {
            get
            {
                return numOfCountries;
            }
            set
            {
                numOfCountries = value;
                OnPropertyChanged("numberOfCountries");
            }
        }
        private int numOfAirlines;
        public int numberOfAirlines
        {
            get
            {
                return numOfAirlines;
            }
            set
            {
                numOfAirlines = value;
                OnPropertyChanged("numberOfAirlines");

            }
        }
        private int numOfFlights;
        public int numberOfFlights
        {
            get
            {
                return numOfFlights;
            }
            set
            {
                numOfFlights = value;
                OnPropertyChanged("numberOfFlights");
            }
        }
        private int numOfTickets;
        public int numberOfTickets
        {
            get
            {
                return numOfTickets;
            }
            set
            {
                numOfTickets = value;
                OnPropertyChanged("numberOfTickets");
            }
        }
        private string message;
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
                OnPropertyChanged("Message");
            }
        }
        private int status;
        public int Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }

        public string Error
        {
            get
            {
                return string.Empty;
            }
        }

        public void OnWork()
        {

            _randomDataGenerator.Total = numberOfAirlines + numberOfCountries + numberOfCustomers
                   + (numberOfFlights * numOfAirlines) + (numberOfTickets * numberOfCustomers);
            _backGround.DoWork += (e, s) =>
            {
                while (_onWorking)
                {
                    if (_randomDataGenerator.ProgressValue <= 100)
                    {
                        Status = _randomDataGenerator.ProgressValue;
                        if (_randomDataGenerator.ProgressValue > 100)
                        {
                            Status = 100;
                        }
                    }
                }
            };

            _backGround.RunWorkerAsync();

        }

        public string this[string propertyName]
        {
            get
            {
                return GetErrorForProperty(propertyName);
            }
        }

        private string GetErrorForProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "numberOfCustomers":
                    if (numberOfCustomers == 0)
                        return "not valid";
                    else
                        return string.Empty;
                case "numberOfTickets":
                    if (numberOfTickets == 0)
                        return "not valid";
                    else
                        return string.Empty;
                case "numberOfCountries":
                    if (numberOfCountries == 0)
                        return "not valid";
                    else
                        return string.Empty;
                case "numberOfAirlines":
                    if (numberOfAirlines == 0)
                        return "not valid";
                    else
                        return string.Empty;
                case "numberOfFlights":
                    if (numberOfFlights == 0)
                        return "not valid";
                    else
                        return string.Empty;
                default:
                    return string.Empty;
            }
        }

        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        public void ExecuteAdd()
        {
            Task.Run(() =>
            {
                _randomDataGenerator.ProgressValue = 0;
                SafeInvoke(OnWork);
                _onWorking = true;
                AddToDB.RaiseCanExecuteChanged();
                ReplaceDB.RaiseCanExecuteChanged();
                _randomDataGenerator.GetCountriesFromApi(numberOfCountries);
                Message = _randomDataGenerator.Message;
                _randomDataGenerator.GetCustomersFromApi(numberOfCustomers);
                Message = _randomDataGenerator.Message;
                _randomDataGenerator.GetRandomAirlines(numberOfAirlines);
                Message = _randomDataGenerator.Message;
                _randomDataGenerator.GetRandomFlights(numberOfFlights);
                Message = _randomDataGenerator.Message;
                _randomDataGenerator.GetRandomTickets(numberOfTickets);
                Message = _randomDataGenerator.Message;
                _onWorking = false;
                AddToDB.RaiseCanExecuteChanged();
                ReplaceDB.RaiseCanExecuteChanged();
            });

        }

        public bool CanExecuteAdd()
        {
            return !_onWorking;
        }

        public void ExecuteReplace()
        {
            Task.Run(() =>
            {
                _randomDataGenerator.ProgressValue = 0;
                _onWorking = true;
                ReplaceDB.RaiseCanExecuteChanged();
                AddToDB.RaiseCanExecuteChanged();
                _randomDataGenerator.DeleteAll();
                ExecuteAdd();
                _onWorking = false;
                ReplaceDB.RaiseCanExecuteChanged();
                AddToDB.RaiseCanExecuteChanged();
            });

        }

        public bool CanExecuteReplace()
        {
            return !_onWorking;
        }

        private void SafeInvoke(Action work)
        {
            if (Dispatcher.CheckAccess())
            {
                work.Invoke();
                return;
            }
            this.Dispatcher.BeginInvoke(work);
        }
    }
}
