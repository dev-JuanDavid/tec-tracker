using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using MongoDB.Driver;
using Owin;
using Quartz;
using System.Linq;
using Queue.Jobs;
using Quartz.Impl;
using Queue.DataBase;

[assembly: OwinStartupAttribute(typeof(Queue.Startup))]
namespace Queue
{
    public partial class Startup
    {
        public async void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();

            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<Alerts>().WithIdentity("execute", "alerts").Build();
            ITrigger trigger = TriggerBuilder.Create().WithIdentity("Alerts_", "alerts").StartNow().WithSimpleSchedule(x => x.WithIntervalInMinutes(5).WithRepeatCount(-1)).Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public void ConfigurationServices(IServiceCollection services)
        {
            services.AddSingleton<IMongoClient, MongoClient>(s =>
            {
                //return new MongoClient("mongodb+srv://tracker:3hQCcdxBkzHDkB2J@cluster0.4xbwb.mongodb.net/MonitorTracker?retryWrites=true&w=majority");
                return new MongoClient("mongodb://localhost:27017");
            });
            services.Add(new ServiceDescriptor(typeof(IRepositorio), new Repositorio()));
        }
    }
}
