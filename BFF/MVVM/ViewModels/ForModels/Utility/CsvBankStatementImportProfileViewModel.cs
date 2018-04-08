using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Managers;
using BFF.MVVM.Models.Native.Utility;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Utility
{
    public interface ICsvBankStatementImportNonProfileViewModel : IDisposable
    {
        IReactiveProperty<string> Header { get; }
        IReactiveProperty<char> Delimiter { get; }
        IReactiveProperty<string> DateFormat { get; }
        IReactiveProperty<string> PayeeFormat { get; }
        IReactiveProperty<bool> ShouldCreateNewPayeeIfNotExisting { get; }
        IReactiveProperty<string> MemoFormat { get; }
        IReactiveProperty<string> SumFormat { get; }
        IReactiveProperty<string> NewProfileName { get; }

        ReactiveCommand SaveNewProfile { get; }

        ReactiveCommand ResetProfile { get; }

        ReactiveCommand SaveToProfile { get; }

        ReactiveCommand RemoveProfile { get; }
    }

    public interface ICsvBankStatementImportProfileViewModel : ICsvBankStatementImportNonProfileViewModel
    {
        IReadOnlyReactiveProperty<string> Name { get; }
    }

    public class CsvBankStatementImportProfileViewModel : ObservableObject, ICsvBankStatementImportProfileViewModel
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public CsvBankStatementImportProfileViewModel(
            ICsvBankStatementImportProfile profile, 
            ICreateCsvBankStatementImportProfile createProfile,
            ICsvBankStatementProfileManager profileManager)
        {
            bool NewProfileCondition(string name)
            {
                return !name.IsNullOrWhiteSpace() && profileManager.Profiles.All(p => p.Name != name.Trim());
            }

            Name = profile.ToReadOnlyReactivePropertyAsSynchronized(p => p.Name,
                ReactivePropertyMode.DistinctUntilChanged)
                .AddHere(_compositeDisposable);
            Header = new ReactivePropertySlim<string>(profile.Header, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            Delimiter = new ReactivePropertySlim<char>(profile.Delimiter, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            DateFormat = new ReactivePropertySlim<string>(profile.DateFormat, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            PayeeFormat = new ReactivePropertySlim<string>(profile.PayeeFormat, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            ShouldCreateNewPayeeIfNotExisting = new ReactivePropertySlim<bool>(
                profile.ShouldCreateNewPayeeIfNotExisting,
                ReactivePropertyMode.DistinctUntilChanged)
                .AddHere(_compositeDisposable);
            NewProfileName = new ReactiveProperty<string>("", ReactivePropertyMode.DistinctUntilChanged);
            MemoFormat = new ReactivePropertySlim<string>(profile.MemoFormat, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            SumFormat = new ReactivePropertySlim<string>(profile.SumFormat, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

            SaveToProfile = new ReactiveCommand().AddHere(_compositeDisposable);
            SaveToProfile.Subscribe(_ =>
            {
                profile.Header = Header.Value;
                profile.Delimiter = Delimiter.Value;
                profile.DateFormat = DateFormat.Value;
                profile.PayeeFormat = PayeeFormat.Value;
                profile.ShouldCreateNewPayeeIfNotExisting = ShouldCreateNewPayeeIfNotExisting.Value;
                profile.MemoFormat = MemoFormat.Value;
                profile.SumFormat = SumFormat.Value;
                profileManager.Save();
            }).AddHere(_compositeDisposable);
            
            SaveNewProfile = NewProfileName
                .Merge(profileManager.Profiles.CollectionChangedAsObservable().Select(_ => NewProfileName.Value))
                .Select(NewProfileCondition)
                .ToReactiveCommand()
                .AddHere(_compositeDisposable);
            SaveNewProfile.Subscribe(_ =>
            {
                if (!NewProfileCondition(NewProfileName.Value)) return;
                profile = createProfile.Create(
                    Header.Value,
                    Delimiter.Value,
                    DateFormat.Value,
                    PayeeFormat.Value,
                    ShouldCreateNewPayeeIfNotExisting.Value,
                    MemoFormat.Value,
                    SumFormat.Value,
                    NewProfileName.Value);
                NewProfileName.Value = "";
            }).AddHere(_compositeDisposable);

            RemoveProfile = new ReactiveCommand().AddHere(_compositeDisposable);
            RemoveProfile.Subscribe(_ =>
                profileManager.Remove(profile.Name)).AddHere(_compositeDisposable);

            ResetProfile = new ReactiveCommand().AddHere(_compositeDisposable);
            ResetProfile.Subscribe(_ =>
            {
                Header.Value = profile.Header;
                Delimiter.Value = profile.Delimiter;
                DateFormat.Value = profile.DateFormat;
                PayeeFormat.Value = profile.PayeeFormat;
                ShouldCreateNewPayeeIfNotExisting.Value = profile.ShouldCreateNewPayeeIfNotExisting;
                MemoFormat.Value = profile.MemoFormat;
                SumFormat.Value = profile.SumFormat;
            }).AddHere(_compositeDisposable);
        }

        public IReadOnlyReactiveProperty<string> Name { get; }
        public IReactiveProperty<string> Header { get; }
        public IReactiveProperty<char> Delimiter { get; }
        public IReactiveProperty<string> DateFormat { get; }
        public IReactiveProperty<string> PayeeFormat { get; }
        public IReactiveProperty<bool> ShouldCreateNewPayeeIfNotExisting { get; }
        public IReactiveProperty<string> MemoFormat { get; }
        public IReactiveProperty<string> NewProfileName { get; }
        public IReactiveProperty<string> SumFormat { get; }
        public ReactiveCommand SaveToProfile { get; }
        public ReactiveCommand RemoveProfile { get; }
        public ReactiveCommand SaveNewProfile { get; }
        public ReactiveCommand ResetProfile { get; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }

    public class CsvBankStatementImportNonProfileViewModel : ObservableObject, ICsvBankStatementImportNonProfileViewModel
    {

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public CsvBankStatementImportNonProfileViewModel(
            IReactiveProperty<string> filePath, 
            ICreateCsvBankStatementImportProfile createProfile,
            ICsvBankStatementProfileManager profileManager, 
            IRxSchedulerProvider schedulerProvider)
        {
            bool NewProfileCondition(string name)
            {
                return !name.IsNullOrWhiteSpace() && profileManager.Profiles.All(p => p.Name != name.Trim());
            }

            bool NotSameCountOrZero(IEnumerable<string> lines, char delimiter)
            {
                int count = lines.First().Count(c => c == delimiter);

                if (count == 0) return true;

                return lines.Any(line => count != line.Count(c => c == delimiter));
            }
            bool SameCountAndNotZero(IEnumerable<string> lines, char delimiter)
            {
                int count = lines.First().Count(c => c == delimiter);

                if (count == 0) return false;

                return lines.All(line => count == line.Count(c => c == delimiter));
            }

            Header = new ReactiveProperty<string>(filePath.Select(path => path != null && File.Exists(path) ? File.ReadLines(path, Encoding.Default).FirstOrDefault() : ""),
                mode: ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            Delimiter = new ReactiveProperty<char>(
                Header
                    .Where(header => !string.IsNullOrWhiteSpace(header))
                    .Where(_ => NotSameCountOrZero(File.ReadLines(filePath.Value, Encoding.Default).Take(5), Delimiter.Value))
                    .Select(_ => new[] { ',', ';', '\t' }
                        .FirstOrDefault(
                            delimiter => SameCountAndNotZero(
                                File.ReadLines(filePath.Value, Encoding.Default).Take(5),
                                delimiter))), schedulerProvider.UI, mode: ReactivePropertyMode.DistinctUntilChanged);
            DateFormat = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            PayeeFormat = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            ShouldCreateNewPayeeIfNotExisting = new ReactivePropertySlim<bool>(
                    true,
                    ReactivePropertyMode.DistinctUntilChanged)
                .AddHere(_compositeDisposable);
            NewProfileName = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged);
            MemoFormat = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            SumFormat = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

            SaveNewProfile = NewProfileName
                .Merge(profileManager.Profiles.CollectionChangedAsObservable().Select(_ => NewProfileName.Value))
                .Select(NewProfileCondition)
                .ToReactiveCommand()
                .AddHere(_compositeDisposable);
            SaveNewProfile.Subscribe(_ =>
            {
                if (!NewProfileCondition(NewProfileName.Value)) return;
                createProfile.Create(
                    Header.Value,
                    Delimiter.Value,
                    DateFormat.Value,
                    PayeeFormat.Value,
                    ShouldCreateNewPayeeIfNotExisting.Value,
                    MemoFormat.Value,
                    SumFormat.Value,
                    NewProfileName.Value);
                NewProfileName.Value = "";
            }).AddHere(_compositeDisposable);

            ResetProfile = new ReactiveCommand().AddHere(_compositeDisposable);
            ResetProfile.Subscribe(_ =>
            {
                Header.Value = "";
                Delimiter.Value = ',';
                DateFormat.Value = "";
                PayeeFormat.Value = "";
                ShouldCreateNewPayeeIfNotExisting.Value = false;
                MemoFormat.Value = "";
                SumFormat.Value = "";
            }).AddHere(_compositeDisposable);
        }
        public IReactiveProperty<string> Header { get; }
        public IReactiveProperty<char> Delimiter { get; }
        public IReactiveProperty<string> DateFormat { get; }
        public IReactiveProperty<string> PayeeFormat { get; }
        public IReactiveProperty<bool> ShouldCreateNewPayeeIfNotExisting { get; }
        public IReactiveProperty<string> MemoFormat { get; }
        public IReactiveProperty<string> NewProfileName { get; }
        public IReactiveProperty<string> SumFormat { get; }
        public ReactiveCommand SaveNewProfile { get; }
        public ReactiveCommand ResetProfile { get; }
        public ReactiveCommand SaveToProfile { get; } = null;
        public ReactiveCommand RemoveProfile { get; } = null;

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
