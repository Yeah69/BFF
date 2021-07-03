using Autofac;
using BFF.Core.Helper;
using BFF.View.Wpf.Helper;
using BFF.View.Wpf.Views;
using MahApps.Metro.Controls.Dialogs;
using MrMeeseeks.ResXToViewModelGenerator;
using Module = Autofac.Module;

namespace BFF.Composition
{
    public class ViewAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(_ => DialogCoordinator.Instance).As<IDialogCoordinator>();

            builder.RegisterType<WpfRxSchedulerProvider>().As<IRxSchedulerProvider>().SingleInstance();

            builder.Register(_ => new CurrentTextsViewModel())
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<MainWindow>().AsSelf().SingleInstance();
        }
    }
}