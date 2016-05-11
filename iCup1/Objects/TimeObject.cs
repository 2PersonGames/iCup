using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iCup1
{
    class TimeObject
    {
        float timer;

        public TimeObject(float time)
        {
            timer = time;
        }
        public float getTime()
        {

            return timer;

        }
        public void Update(float elapsedTime)
        {
            timer -= elapsedTime;
        }
        public string Draw()
        {
            string output;
            float temp;

            if (timer > 60000)
            {
                temp = timer - 60000;
                if (temp < 10000 && temp > 1000)
                {
                    output = "1 min 0" + temp.ToString().Substring(0, 1) + " seconds";
                }
                else if (temp <= 1000)
                {
                    output = "1 min 00 seconds";
                }
                else
                {
                    output = "1 min " + temp.ToString().Substring(0, 2) + " seconds";
                }

            }
            else
            {
                if (timer < 10000 && timer > 1000)
                {
                    output = "0 mins 0" + timer.ToString().Substring(0, 1) + " seconds";
                }
                else if (timer <= 1000)
                {
                    output = " 0 mins 00 seconds";
                }
                else
                {
                    output = "0 mins " + timer.ToString().Substring(0, 2) + " seconds";
                }
            }
            return output;
        }
    }
}
