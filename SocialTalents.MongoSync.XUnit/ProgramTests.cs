using SocialTalents.MongoSync.Console;
using System;
using Xunit;

namespace SocialTalents.MongoSync.XUnit
{
    public class ProgramTests
    {
        [Fact]
        public void Hello()
        {

            Program.Main(null);
        }
    }
}
