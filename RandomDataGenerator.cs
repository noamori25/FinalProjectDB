using Newtonsoft.Json;
using ProjectManagmentSystem.DAO;
using ProjectManagmentSystem.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static DBgenerator.ApiModel;

namespace DBgenerator
{
    internal class RandomDataGenerator
    {
        private HttpClient client = new HttpClient();
        private string url = "https://randomuser.me/api";
        private Random _random;
        private string _characters = "abcdefghijklmnop";
        private int _randomNum;

        internal string Message { get; set; }
        internal int ProgressValue { get; set; }
        internal int Total { get; set; }

        private CountryDAOMSSQL _countryDao;
        private CustomerDAOMSSQL _customerDao;
        private AirlineDAOMSSQL _airlineDao;
        private FlightDAOMSSQL _flightDao;
        private TicketDAOMSSQL _ticketDao;
        private List<Country> _countries;
        private List<Customer> _customers;
        private List<AirlineCompany> _airlines;
        private List<Flight> _flights;
        private List<Ticket> _tickets;


        public RandomDataGenerator()
        {
            _random = new Random();
            _countryDao = new CountryDAOMSSQL();
            _customerDao = new CustomerDAOMSSQL();
            _airlineDao = new AirlineDAOMSSQL();
            _flightDao = new FlightDAOMSSQL();
            _ticketDao = new TicketDAOMSSQL();
            _countries = new List<Country>();
            _customers = new List<Customer>();
            _airlines = new List<AirlineCompany>();
            _flights = new List<Flight>();
            _tickets = new List<Ticket>();
        }

        internal void GetCountriesFromApi(int number)
        {
            int counter = 0;
            try
            {
                _countries = _countryDao.GetAll().ToList();
                HttpResponseMessage response = client.GetAsync(url + "?results=" + number).Result;

                for (int i = 0; i < number; i++)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        Country c = new Country();
                        var content = response.Content.ReadAsStringAsync().Result;

                        RootObject r = JsonConvert.DeserializeObject<RootObject>(content);
                        if (!_countries.Any(country => country.CountryName == r.results[i].location.state))
                        {
                            c.CountryName = r.results[i].location.state;
                            _countryDao.Add(c);
                            counter++;
                        }

                        ProgressValue += Convert.ToInt32(1.0 / Total * 100);
                    }
                }
                _countries = _countryDao.GetAll().ToList();
                Message = $"Created Countries {counter}/{number}\n";
            }
            catch (Exception e)
            {
                Message += $"{e.ToString()}\n";

            }
        }

        internal void GetCustomersFromApi(int number)
        {
            int counter = 0;
            try
            {
                List<Customer> customerFromDB = _customerDao.GetAll().ToList();
                HttpResponseMessage response = client.GetAsync(url + "?results=" + number).Result;
                for (int i = 0; i < number; i++)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        Customer c = new Customer();
                        var content = response.Content.ReadAsStringAsync().Result;
                        RootObject r = JsonConvert.DeserializeObject<RootObject>(content);
                        if (!customerFromDB.Any(customer => customer.UserName == r.results[i].login.username))
                        {
                            c.FirstName = r.results[i].name.first;
                            c.LastName = r.results[i].name.last;
                            c.Password = r.results[i].login.password;
                            c.UserName = r.results[i].login.username;
                            c.PhoneNo = r.results[i].phone;
                            c.Address = r.results[i].location.street + " " + r.results[0].location.city;
                            c.CreditCardNumber = r.results[i].location.postcode;
                            _customerDao.Add(c);
                            _customers.Add(c);
                            counter++;
                        }
                        ProgressValue += Convert.ToInt32(1.0 / Total * 100);
                    }
                }
                Message += $"Created Customers {counter}/{number}\n";

            }
            catch (Exception e)
            {
                Message += $"{e.ToString()}\n";
            }


        }

        internal void GetRandomAirlines(int number)
        {
            int counter = 0;
            try
            {
                long LastId = _airlineDao.GetAll().Any() ? _airlineDao.GetAll().ToList().Last().Id : 1;
                for (int i = 0; i < number; i++)
                {
                    AirlineCompany ac = new AirlineCompany();
                    _randomNum = _random.Next(1, 6);
                    StringBuilder randomString = new StringBuilder(_randomNum);
                    for (int j = 0; j < _randomNum; j++)
                    {
                        randomString.Append(_characters[_random.Next(_characters.Length)]);
                    }
                    ac.AirlineName = randomString.ToString() + LastId.ToString();
                    _randomNum = _random.Next(1, 6);
                    for (int j = 0; j < _randomNum; j++)
                    {
                        randomString.Append(_characters[_random.Next(_characters.Length)]);
                    }
                    ac.UserName = randomString.ToString();
                    ac.Password = randomString.ToString();
                    _randomNum = _random.Next(0, _countries.Count);
                    ac.CountryCode = _countries[_randomNum].Id;
                    _airlineDao.Add(ac);
                    _airlines.Add(ac);
                    counter++;
                    LastId++;
                    ProgressValue += Convert.ToInt32(1.0 / Total * 100);
                }
                Message += $"Created Airline Comapnies {counter}/{number}\n";
            }
            catch (Exception e)
            {
                Message += $"{e.ToString()}\n";
            }


        }

        internal void GetRandomFlights(int number)
        {
            int counter = 0;
            try
            {
                for (int i = 0; i < _airlines.Count; i++)
                {
                    AirlineCompany a = _airlineDao.GetAirlineByUserName(_airlines[i].UserName);
                    for (int j = 0; j < number; j++)
                    {
                        Flight f = new Flight();
                        f.DepartureTime = CreateRandomDate();
                        f.LandingTime = CreateRandomDate();
                        _randomNum = _random.Next(100, 350);
                        f.RemainingTickets = _randomNum;
                        _randomNum = _random.Next(0, _countries.Count);
                        f.DestinationCountryCode = _countries[_randomNum].Id;
                        _randomNum = _random.Next(0, _countries.Count);
                        f.OriginCountryCode = _countries[_randomNum].Id;
                        f.AirlineCompanyId = a.Id;
                        _flightDao.Add(f);
                        _flights.Add(f);
                        counter++;
                        ProgressValue += Convert.ToInt32(1.0 / Total * 100);
                    }
                }
                Message += $"Created Airline Flights {counter}/{number} per airline company\n";
                _flights = _flightDao.GetAll().ToList();
            }
            catch (Exception e)
            {
                Message += $"{e.ToString()}\n";
            }

        }

        internal void GetRandomTickets(int number)
        {
            int counter = 0;
            try
            {
                for (int i = 0; i < _customers.Count; i++)
                {
                    Customer c = _customerDao.GetCustomerByUserName(_customers[i].UserName);
                    for (int j = 0; j < number; j++)
                    {
                        Ticket t = new Ticket();
                        t.CustomerId = c.Id;
                        _randomNum = _random.Next(0, _flights.Count);
                        t.FlightId = _flights[_randomNum].Id;
                        if (!_tickets.Any(ticket => ticket.FlightId == t.FlightId && ticket.CustomerId == t.CustomerId))
                        {
                            _ticketDao.Add(t);
                            _tickets.Add(t);
                            counter++;
                        }
                        ProgressValue += Convert.ToInt32(1.0 / Total * 100);

                    }
                }
                Message += $"Created Airline Tickets {counter}/{number} per customer\n";
            }
            catch (Exception e)
            {
                Message += $"{e.ToString()}\n";
            }

        }

        private DateTime CreateRandomDate()
        {

            return DateTime.Now.AddDays(_random.Next(1000));
        }

        internal void DeleteAll()
        {
            Message += "Delete the DataBase";
            DeleteDataDAO deleteAll = new DeleteDataDAO();
            deleteAll.RemoveAll();
            _airlines.Clear();
            _countries.Clear();
            _customers.Clear();
            _tickets.Clear();
            _flights.Clear();
        }
    }
}
