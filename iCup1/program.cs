using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace icup1
{
    class program
    {
        static void Main()
        {
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
}
