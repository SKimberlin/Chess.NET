using Chess.Model.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Model.Data
{
    public class GameSettings
    {
        public IRulebook Rulebook { get; set; }

        private static GameSettings _instance;
        public static GameSettings Instance => _instance ??= new GameSettings();
    }
}
