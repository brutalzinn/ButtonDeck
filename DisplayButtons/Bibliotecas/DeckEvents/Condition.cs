﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DisplayButtons.Bibliotecas.DeckEvents
{
   public class Condition
    {
        public bool timer_interval { get; set; }
        public bool timer_extact { get; set; }
        public bool timer_after { get; set; }
        public bool timer_none { get; set; }
        public string lua_path { get; set; }

        public string timer_interval_start { get; set; }

        public string timer_interval_end { get; set; }

        public string timer_exact { get; set; }

        public Condition(){


            }
        public Condition(string timer_interval_a, string timer_interval_b,string timer_exact)
        {


        }
        public bool CheckTimerInterval()
        {
            bool result= false;
            TimeSpan start = TimeSpan.Parse(timer_interval_start); // 10 PM
            TimeSpan end = TimeSpan.Parse(timer_interval_end);   // 2 AM
            TimeSpan now = DateTime.Now.TimeOfDay;

            if (start <= end)
            {
                // start and stop times are in the same day
                if (now >= start && now <= end)
                {
                    result = true;
                }
            }
            else
            {
                // start and stop times are in different days
                if (now >= start || now <= end)
                {
                    result =  true;
                }
            }
            return result;

        }

        public bool CheckCondition()
        {
           return CheckTimerInterval();


        }
    }
}