using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ExtensionMethod
{
    public static class Event
    {


        public static IEnumerable GetEvents()
        {
            ObjectCache cache = MemoryCache.Default;

            if (cache.Contains(CacheKey))
                return (IEnumerable)cache.Get(CacheKey);
            else
            {
                IEnumerable availableStocks = this.GetDefaultStocks();

                // Store data in the cache    
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
                cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddHours(1.0);
                cache.Add(CacheKey, availableStocks, cacheItemPolicy);

                return availableStocks;
            }
        }
    }
}
namespace Viagogo
{

    public class Event
    {
        public string Name { get; set; }
        public string City { get; set; }


      
    }
    public class Customer
    {
        public string Name { get; set; }
        public string City { get; set; }
    }
    public class Solution
    {
        static void Main(string[] args)
        {
            var events = new List<Event>{
new Event{ Name = "Phantom of the Opera", City = "New York"},
new Event{ Name = "Metallica", City = "Los Angeles"},
new Event{ Name = "Metallica", City = "New York"},
new Event{ Name = "Metallica", City = "Boston"},
new Event{ Name = "LadyGaGa", City = "New York"},
new Event{ Name = "LadyGaGa", City = "Boston"},
new Event{ Name = "LadyGaGa", City = "Chicago"},
new Event{ Name = "LadyGaGa", City = "San Francisco"},
new Event{ Name = "LadyGaGa", City = "Washington"} };
            //1. find out all events that arein cities of customer // then add to email.
            var customer = new Customer { Name = "Mr. Fake", City = "New York" };

            var customers = new List<Customer>{
new Customer{ Name = "Mr.Los Angeles", City = "Los Angeles"},
new Customer{ Name = "Mr.New York", City = "New York"},
new Customer{ Name = "Mr.Chicago", City = "Chicago"},
new Customer{ Name = "Mr.San Francisco", City = "San Francisco"},
new Customer{ Name = "Mr.Washington", City = "Washington"} };
            //        Questions:
            //            1.Imagine this is the interface or an API, so you don't care how he works. You just need to know that you need to call this function to add an event to the email that you want to send to the customer. The function takes two parameters as the input: the customer and the event. So thefirst task here is to prepare the email for this customer in the main function to send all the events.
            //Write a code to add all events in the customer's location to the email. Considering the objects shared above:
            //1.  What should be your approach to getting the list of events?
            var query = (from selectedevents in customers
                         join selctedcustomer in events
                              on selectedevents.City equals selctedcustomer.City into temp
                         from selctedcustomer in temp.DefaultIfEmpty()
                         select new
                         {
                             Name = selectedevents.Name,
                             City = selectedevents.City,
                             CustomerName = selctedcustomer.Name
                         }).Distinct();

            var results = customers.Join(events,
  cu => cu.City,
  ev => ev.City,
  (customer, eventss) => new { Name = eventss.Name, City = eventss.City, Customer = customer.Name }
);
            //var query = from result in customers  where result.Contains("New York") select result;

            //2.  How would you call the AddToEmail method in order to send the events in an email?
            foreach (var item in query)
            {
                AddToEmail(new Customer { Name = item.CustomerName, City = item.City }, new Event { Name = item.Name, City = item.City });
            }
            //3.  What is the expected output if we only have the client John Smith?
            var output = new List<Event>{
new Event{ Name = "Phantom of the Opera", City = "New York"},
new Event{ Name = "Metallica", City = "New York"},
new Event{ Name = "LadyGaGa", City = "New York"}};

            //4.  Do you believe there is a way to improve the code you first wrote?
            (from selctedcustomer in customers
             join selectedevents in events
                  on selctedcustomer.City equals selectedevents.City
             select new
             {
                 Name = selectedevents.Name,
                 City = selectedevents.City,
                 CustomerName = selctedcustomer.Name
             }).ToList()
    .ForEach(x =>
    {
        AddToEmail(new Customer { Name = x.CustomerName, City = x.City }, new Event { Name = x.Name, City = x.City });
    });


            //2. We want to send the events occurring in the client’s city, but also the ones that occur in the nearest city.For this, there’s another function called GetDistance.This function takes two inputs: from city and to city to calculate the distance.If you put from city and to city as the same city, the distance will be 0 (zero). You will need to send 5 events to the customer, so you need to get the closest to them.
            //Write a code to add the 5 closest events to the customer's location to the email.
            //1.  What should be your approach to getting the distance between the customer’s city and the other cities on the list?

            //2.  How would you get the 5 closest events and how would you send them to the client in an email?
            var lst = (from selectedevents in events
                       orderby GetDistance(customer.City, selectedevents.City) ascending
                       select selectedevents).Take(5).ToList();
            int count = 0;
            customers.ForEach(customer =>
            {

                (from selectedevents in events

                 orderby GetDistance(customer.City, selectedevents.City)
                 select selectedevents).Take(5).ToList().ForEach(x =>
       {
           Console.WriteLine(count++);
           AddToEmail(new Customer { Name = customer.Name, City = x.City }, new Event { Name = x.Name, City = x.City });
       });

            });

            //3.  What is the expected output if we only have the client John Smith?
            var secondOutput = new List<Event>{
new Event{ Name = "Phantom of the Opera", City = "New York"},
new Event{ Name = "Metallica", City = "New York"},
new Event{ Name = "LadyGaGa", City = "New York"},
new Event{ Name = "LadyGaGa", City = "Boston"},
new Event{ Name = "Metallica", City = "Boston"}
            };
            //4.  Do you believe there is a way to improve the code you first wrote?
            customers.ForEach(customer =>
            {

                (from selectedevents in events

                 orderby GetDistance(customer.City, selectedevents.City)
                 select selectedevents).Take(5).ToList().ForEach(x =>
                 {
                     Console.WriteLine(count++);
                     AddToEmail(new Customer { Name = customer.Name, City = x.City }, new Event { Name = x.Name, City = x.City });
                 });

            });



            //3. If the GetDistance method is an API call which could fail or is too expensive, how will u improve the code written in 2? Write the code.
         //Cache the Data 
            
            customers.ForEach(customer =>
            {

                (from selectedevents in events

                 orderby Retry.Do(() => GetDistance(customer.City, selectedevents.City), TimeSpan.FromSeconds(1))
                 select selectedevents).Take(5).ToList().ForEach(x =>
                      {
                          Console.WriteLine(count++);
                          AddToEmail(new Customer { Name = customer.Name, City = x.City }, new Event { Name = x.Name, City = x.City });
                      });

            });

            Retry.Do(() => GetDistance("", ""), TimeSpan.FromSeconds(1));
            //4. If the GetDistance method can fail, we don't want the process to fail. What can be done? Code it. (Ask clarifying questions to be clear about what is expected business-wise)

            //5. If we also want to sort the resulting events by other fields like price, etc.to determine which ones to send to the customer, how would you implement it? Code it.



            // 1. TASK

            /*
            * We want you to send an email to this customer with all events in their city
            * Just call AddToEmail(customer, event) for each event you think they should get */
        }
        // You do not need to know how these methods work
        static void AddToEmail(Customer c, Event e, int? price = null)
        {
            var distance = GetDistance(c.City, e.City);
            Console.Out.WriteLine($"{c.Name}: {e.Name} in {e.City}" + (distance > 0 ? $" ({distance} miles away)" : "")
    + (price.HasValue ? $" for ${price}" : ""));
        }
        static int GetPrice(Event e)
        {
            return (AlphebiticalDistance(e.City, "") + AlphebiticalDistance(e.Name, "")) / 10;
        }
        static int GetDistance(string fromCity, string toCity)
        {
            return AlphebiticalDistance(fromCity, toCity);
        }
        private static int AlphebiticalDistance(string s, string t)
        {
            var result = 0; var i = 0;
            for (i = 0; i < Math.Min(s.Length, t.Length); i++)
            {
                // Console.Out.WriteLine($"loop 1 i={i} {s.Length} {t.Length}"); result += Math.Abs(s[i] - t[i]);
            }
            for (; i < Math.Max(s.Length, t.Length); i++)
            {
                // Console.Out.WriteLine($"loop 2 i={i} {s.Length} {t.Length}"); result += s.Length > t.Length ? s[i] : t[i];
            }
            return result;
        }

        /*
       var customers = new List<Customer>{
       new Customer{ Name = "Nathan", City = "New York"},
       new Customer{ Name = "Bob", City = "Boston"},
       new Customer{ Name = "Cindy", City = "Chicago"},
       new Customer{ Name = "Lisa", City = "Los Angeles"} };
       */
    }
    public static class Retry
    {
        public static void Do(
            Action action,
            TimeSpan retryInterval,
            int maxAttemptCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, maxAttemptCount);
        }

        public static T Do<T>(
            Func<T> action,
            TimeSpan retryInterval,
            int maxAttemptCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    if (attempted > 0)
                    {
                        Thread.Sleep(retryInterval);
                    }
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException(exceptions);
        }
    }
}