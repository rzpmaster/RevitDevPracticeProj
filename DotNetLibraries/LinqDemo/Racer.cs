﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqDemo
{
    /// <summary>
    /// 车手
    /// </summary>
    class Racer : IComparable<Racer>, IFormattable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Wins { get; set; }
        public string Country { get; set; }
        public int Starts { get; set; }
        public IEnumerable<string> Cars { get; }
        public IEnumerable<int> Years { get; }

        public Racer(string firstName, string lastName, string country,
            int starts, int wins, IEnumerable<int> years, IEnumerable<string> cars)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Country = country;
            this.Starts = starts;
            this.Wins = wins;
            this.Years = years != null ? new List<int>(years) : new List<int>();
            this.Cars = cars != null ? new List<string>(cars) : new List<string>();
        }

        public Racer(string firstName, string lastName, string country, int starts, int wins)
            : this(firstName, lastName, country, starts, wins, null, null)
        {
        }

        public override string ToString() => FirstName + " " + LastName;

        public int CompareTo(Racer other) => LastName.CompareTo(other?.LastName);

        public string ToString(string format) => ToString(format, null);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            switch (format)
            {
                case null:
                case "N":
                    return "None";
                case "F":
                    return FirstName;
                case "L":
                    return LastName;
                case "C":
                    return Country;
                case "S":
                    return Starts.ToString();
                case "W":
                    return Wins.ToString();
                case "A":
                    return $"{FirstName} {LastName},{Country}; start:{Starts}, wins:{Wins}";
                default:
                    throw new FormatException($"Format {format} not supproted");
            }
        }
    }

    /// <summary>
    /// 车队
    /// </summary>
    class Team
    {
        public string Name { get; }
        public IEnumerable<int> Years { get; }

        public Team(string name, params int[] years)
        {
            this.Name = name;
            this.Years = years != null ? new List<int>(years) : new List<int>();
        }
    }

    /// <summary>
    /// 锦标赛
    /// </summary>
    class Championship
    {
        public int Year { get; set; }
        public string First { get; set; }
        public string Second { get; set; }
        public string Third { get; set; }
    }

    /// <summary>
    /// 车手信息
    /// </summary>
    class RacerInfo
    {
        public int Year { get; set; }
        public int Positon { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
