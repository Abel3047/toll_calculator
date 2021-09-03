using System;
using System.Collections.Generic;
using CommercialRegistration;
using ConsumerVehicleRegistration;
using LiveryRegistration;

namespace toll_calculator_and_SwitchLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }

    public class TollCalculator
    {
        #region CalculateToll
        public decimal CalculateToll(object vehicle) =>
            vehicle switch
            {
                Car c => 2.00m,
                Taxi t => 3.50m,
                Bus b => 10.00m,
                DeliveryTruck d => 10.00m,
                { } => throw new ArgumentException(message: "Not a known vehicle type", paramName: nameof(vehicle)),
                null => throw new ArgumentNullException(nameof(vehicle))
            };
        public decimal CalculateToll(object vehicle, bool occupancyPricing) =>
            vehicle switch
            {
                //This below is how it will look if we didn't use the nested switch expression
                /*
                 Car { Passengers: 0 } => 2.00m + 0.50m,
                Car { Passengers: 1 } => 2.0m,
                Car { Passengers: 2 } => 2.0m - 0.50m,
                Car c => 2.00m - 1.0m,

                Taxi { Fares: 0 } => 3.50m + 1.00m,
                Taxi { Fares: 1 } => 3.50m,
                Taxi { Fares: 2 } => 3.50m - 0.50m,
                Taxi t => 3.50m - 1.00m,

                Bus b when ((double)b.Riders / (double)b.Capacity) < 0.50 => 5.00m + 2.00m,
                Bus b when ((double)b.Riders / (double)b.Capacity) > 0.90 => 5.00m - 1.00m,
                Bus b => 5.00m,

                DeliveryTruck t when (t.GrossWeightClass > 5000) => 10.00m + 5.00m,
                DeliveryTruck t when (t.GrossWeightClass < 3000) => 10.00m - 2.00m,
                DeliveryTruck t => 10.00m,

                { } => throw new ArgumentException(message: "Not a known vehicle type", paramName: nameof(vehicle)),
                null => throw new ArgumentNullException(nameof(vehicle))
                 */

                Car c => c.Passengers switch
                {
                    0 => 2.00m + 0.5m,
                    1 => 2.0m,
                    2 => 2.0m - 0.5m,
                    _ => 2.00m - 1.0m //here we use a discard, a placeholder that means it isn't really used but the syntax asks that it must be stated. It also acts as a short code for default
                },

                Taxi t => t.Fares switch
                {
                    0 => 3.50m + 1.00m,
                    1 => 3.50m,
                    2 => 3.50m - 0.50m,
                    _ => 3.50m - 1.00m
                },

                Bus b when ((double)b.Riders / (double)b.Capacity) < 0.50 => 5.00m + 2.00m,
                Bus b when ((double)b.Riders / (double)b.Capacity) > 0.90 => 5.00m - 1.00m,
                Bus b => 5.00m,

                DeliveryTruck t when (t.GrossWeightClass > 5000) => 10.00m + 5.00m,
                DeliveryTruck t when (t.GrossWeightClass < 3000) => 10.00m - 2.00m,
                DeliveryTruck t => 10.00m,

                { } => throw new ArgumentException(message: "Not a known vehicle type", paramName: nameof(vehicle)),
                null => throw new ArgumentNullException(nameof(vehicle))
            };

        #endregion

        /*
         For the final feature, the toll authority wants to add time sensitive peak pricing. During the morning and evening rush hours, the tolls are doubled. 
        That rule only affects traffic in one direction: inbound to the city in the morning, and outbound in the evening rush hour. During other times during the workday, 
        tolls increase by 50%. Late night and early morning, tolls are reduced by 25%. During the weekend, it's the normal rate, regardless of the time. 
        You could use a series of if and else statements to express this using the following code:
        */
        public decimal PeakTimePremiumIfElse(DateTime timeOfToll, bool inbound)
        {
            //Alot of my code looks like this, whats the alternative
            if ((timeOfToll.DayOfWeek == DayOfWeek.Saturday) ||
                (timeOfToll.DayOfWeek == DayOfWeek.Sunday))
            {
                return 1.0m;
            }
            else
            {
                int hour = timeOfToll.Hour;
                if (hour < 6)
                {
                    return 0.75m;
                }
                else if (hour < 10)
                {
                    if (inbound)
                    {
                        return 2.0m;
                    }
                    else
                    {
                        return 1.0m;
                    }
                }
                else if (hour < 16)
                {
                    return 1.5m;
                }
                else if (hour < 20)
                {
                    if (inbound)
                    {
                        return 1.0m;
                    }
                    else
                    {
                        return 2.0m;
                    }
                }
                else // Overnight
                {
                    return 0.75m;
                }
            }
        }

        /*
         The preceding code does work correctly, but isn't readable. You have to chain through all the input cases and the nested if statements to reason about the code.
        Instead, you'll use pattern matching for this feature, but you'll integrate it with other techniques. 
        You could build a single pattern match expression that would account for all the combinations of direction, day of the week, and time.
        The result would be a complicated expression. It would be hard to read and difficult to understand. That makes it hard to ensure correctness. 
        Instead, combine those methods to build a tuple of values that concisely describes all those states. 
        Then use pattern matching to calculate a multiplier for the toll. The tuple contains three discrete conditions:

        -The day is either a weekday or a weekend.
        -The band of time when the toll is collected.
        -The direction is into the city or out of the city

        There are 16 different combinations of the three variables. By combining some of the conditions, you'll simplify the final switch expression.
         */
        #region Time
        private static bool IsWeekDay(DateTime timeOfToll) =>
            timeOfToll.DayOfWeek switch
            {
                DayOfWeek.Monday => true,
                DayOfWeek.Tuesday => true,
                DayOfWeek.Wednesday => true,
                DayOfWeek.Thursday => true,
                DayOfWeek.Friday => true,
                DayOfWeek.Saturday => false,
                DayOfWeek.Sunday => false,
                _ => true
            };
        private static bool IsWeekDay(DateTime timeOfToll, bool therealme) =>
              timeOfToll.DayOfWeek switch
              {
                  DayOfWeek.Saturday => false,
                  DayOfWeek.Sunday => false,
                  _ => true
              };

        private enum TimeBand
        {
            MorningRush,
            Daytime,
            EveningRush,
            Overnight
        }

        private static TimeBand GetTimeBand(DateTime timeOfToll) =>
            timeOfToll.Hour switch
            {
                < 6 or > 19 => TimeBand.Overnight,
                < 10 => TimeBand.MorningRush,
                < 16 => TimeBand.Daytime,
                _ => TimeBand.EveningRush,
            };
        #endregion

        public decimal PeakTimePremiumFull(DateTime timeOfToll, bool inbound) =>
    (IsWeekDay(timeOfToll), GetTimeBand(timeOfToll), inbound) switch
    {
        (true, TimeBand.MorningRush, true) => 2.00m,
        (true, TimeBand.MorningRush, false) => 1.00m,
        (true, TimeBand.Daytime, true) => 1.50m,
        (true, TimeBand.Daytime, false) => 1.50m,
        (true, TimeBand.EveningRush, true) => 1.00m,
        (true, TimeBand.EveningRush, false) => 2.00m,
        (true, TimeBand.Overnight, true) => 0.75m,
        (true, TimeBand.Overnight, false) => 0.75m,
        (false, TimeBand.MorningRush, true) => 1.00m,
        (false, TimeBand.MorningRush, false) => 1.00m,
        (false, TimeBand.Daytime, true) => 1.00m,
        (false, TimeBand.Daytime, false) => 1.00m,
        (false, TimeBand.EveningRush, true) => 1.00m,
        (false, TimeBand.EveningRush, false) => 1.00m,
        (false, TimeBand.Overnight, true) => 1.00m,
        (false, TimeBand.Overnight, false) => 1.00m,
        _ => 1.00m
    };
        //the above cod, does not finnesse, too clunky
        /*The above code works, but it can be simplified. All eight combinations for the weekend have the same toll. You can replace all eight with the following line:
    (false, _, _) => 1.0m,

    
    Both inbound and outbound traffic have the same multiplier during the weekday daytime and overnight hours. Those four switch arms can be replaced with the following two lines
    (true, TimeBand.Overnight, _) => 0.75m,
(true, TimeBand.Daytime, _)   => 1.5m,

    The code should look like the following code after those two changes:
*/

        public decimal PeakTimePremium(DateTime timeOfToll, bool inbound) =>
    (IsWeekDay(timeOfToll), GetTimeBand(timeOfToll), inbound) switch
    {
        (true, TimeBand.MorningRush, true) => 2.00m,
        (true, TimeBand.Daytime, _) => 1.50m,
        (true, TimeBand.EveningRush, false) => 2.00m,
        (true, TimeBand.Overnight, _) => 0.75m,
        _ => 1.00m //this is cause everything else is 1.00
    };



        /*
         Here we have a return type being a tuple and the parameter also being a tuple. This example shows a great way to compare many values and give concise results.
        It acts like a really compact if else statement. So when ever you find that you are getting 3 or more if else statements, use this
         */
        public (string place, string Name, string colour, string message) Participant((int Position, string name, double height, bool female) athelete) =>
            athelete switch
            {
                (1, not "Lesego", not < 150.0, _) => ("First", athelete.name, "Gold", "Congragulations, World Champion"),
                (1, not "Lesego", < 150, _) => ("First", athelete.name, "Gold", $"Amazing, with your height you are the new guru God!! All Hail {athelete.name}!!"),
                // you have to put the above statement above/away from the lesego branch cause it wont compute below it
                
                (_,_,200.0,true)=>(athelete.Position.ToString(),athelete.name,null,"She is 200 cm"), //this would have shown up way too many times in multiple if else brackets
                (2,_,_,_)=>("Second",athelete.name,"Silver", "Congragulations! Incredible achievement"),
                (3,_,_,_)=>("Third",athelete.name,"Bronze", "Congragulations, you are one of the best in the world"),
                (<=3,_,_,true)=>(null,athelete.name,null, $"{athelete.name}, {athelete.name.ToUpper()}!, " +
                "Please tell us, how does it feel to be a leading woman in a sport that has only recenty had women participate along men"),
                (1,"Lesego",_,_)=>(null, athelete.name, "Black",$"{athelete.name.ToUpper()}, you stand undefeated yet again, What say you to your disciples?"),
                (>3,_,_,false)=>(null, athelete.name,null,$" We believe in you {athelete.name}!"),
                (>3,_,_,true)=>(null, athelete.name,null,$"You go girl! stomp the competition {athelete.name}"),
                (0, null,0,_)=> throw new ArgumentNullException("Did noone compete?"),
                var someObject => throw new ArgumentException("That was just some fast random object")
            };
        //Note how we used all the variables in the tuple parameter, this should be the case always. Otherwise its just a waste


        public void Acheivement()
        {
            List<(string place, string Name, string colour, string message)> SportsLiveCommentary = new List<(string place, string Name, string colour, string message)>
            {
                Participant((4, "Leon", 179, true)), Participant((1, "Lesego", 179, true)), Participant((6, "Michelle", 179, true)),
                Participant((2, "Abel", 188, false)),Participant((3, "Clark Kent", 201, false)),Participant((2, "She-Hulk", 212, true))
            };
            foreach (var item in SportsLiveCommentary)
            {
                Console.WriteLine(item);
            }
        }

    }

}
