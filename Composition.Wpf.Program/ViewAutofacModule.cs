using Autofac;
using BFF.View.Wpf.Helper;
using BFF.View.Wpf.Views;
using MahApps.Metro.Controls.Dialogs;
using Module = Autofac.Module;

namespace BFF.Composition.Wpf.Program
{
    public class ViewAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(_ => DialogCoordinator.Instance).As<IDialogCoordinator>();

            builder.RegisterType<WpfRxSchedulerProvider>().AsImplementedInterfaces().SingleInstance(); 

            builder.RegisterType<MainWindow>().AsSelf().SingleInstance();
        }
    }
}