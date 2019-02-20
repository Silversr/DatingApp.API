using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }




        //Learn for async
        public async Task<DbSet<Value>> GetValuesAsync()
        {
            Console.WriteLine("Start Get Value");
            //Thread.Sleep(3000);
            var t = Task.Run(() => { Console.WriteLine("start sleep"); Task.Delay(3000); Console.WriteLine("stop sleep"); });
            Console.WriteLine("DoIndepentWorks");
            await t;
            Console.WriteLine("After await t, should after stop sleep");
            return this.Values;
        }
    }
}
