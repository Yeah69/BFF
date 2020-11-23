using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using BFF.Core.Helper;
using BFF.Model.Models.Utility;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels.Utility
{
    public interface ICsvBankStatementImportNonProfileViewModel : IDisposable
    {
        IReactiveProperty<string> Header { get; }
        IReactiveProperty<char> Delimiter { get; }
        IReactiveProperty<string?> DateSegment { get; }
        IReactiveProperty<CultureInfo?> DateLocalization { get; }
        IReactiveProperty<string> PayeeFormat { get; }
        IReactiveProperty<bool> ShouldCreateNewPayeeIfNotExisting { get; }
        IReactiveProperty<string> MemoFormat { get; }
        IReactiveProperty<string> SumFormula { get; }
        IReactiveProperty<CultureInfo?> SumLocalization { get; }
        IReactiveProperty<string> NewProfileName { get; }

        IReadOnlyReactiveProperty<string[]?> Segments { get; }

        IRxRelayCommand SaveNewProfile { get; }

        IRxRelayCommand ResetProfile { get; }

        IRxRelayCommand? SaveToProfile { get; }

        IRxRelayCommand? RemoveProfile { get; }
    }

    public interface ICsvBankStatementImportProfileViewModel : ICsvBankStatementImportNonProfileViewModel
    {
        IReadOnlyReactiveProperty<string> Name { get; }
    }

    internal class CsvBankStatementImportProfileViewModel : ViewModelBase, ICsvBankStatementImportProfileViewModel
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public CsvBankStatementImportProfileViewModel(
            ICsvBankStatementImportProfile profile, 
            ICreateCsvBankStatementImportProfile createProfile,
            ICsvBankStatementProfileManager profileManager)
        {
            bool NewProfileCondition(string name)
            {
                return !name.IsNullOrWhitespace() && profileManager.Profiles.All(p => p.Name != name.Trim());
            }

            Name = profile.ToReadOnlyReactivePropertyAsSynchronized(p => p.Name,
                ReactivePropertyMode.DistinctUntilChanged)
                .CompositeDisposalWith(_compositeDisposable);
            Header = new ReactivePropertySlim<string>(profile.Header, ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);
            Delimiter = new ReactivePropertySlim<char>(profile.Delimiter, ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);

            Segments = Header.Select(_ => Unit.Default)
                .Merge(Delimiter.Select(_ => Unit.Default))
                .Select(_ => Header.Value.Split(Delimiter.Value))
                .ToReadOnlyReactiveProperty(Header.Value.Split(Delimiter.Value));
            DateSegment = Segments.Select(s => s?.FirstOrDefault(seg => seg == profile.DateSegment) ?? s?.FirstOrDefault()).ToReactiveProperty();

            DateLocalization = profile
                .ObserveProperty(p => p.DateLocalization)
                .Select(CultureInfo.GetCultureInfo)
                .ToReactiveProperty(mode: ReactivePropertyMode.DistinctUntilChanged)
                .CompositeDisposalWith(_compositeDisposable);
            SumLocalization = profile
                .ObserveProperty(p => p.SumLocalization)
                .Select(CultureInfo.GetCultureInfo)
                .ToReactiveProperty(mode: ReactivePropertyMode.DistinctUntilChanged)
                .CompositeDisposalWith(_compositeDisposable);

            PayeeFormat = new ReactivePropertySlim<string>(profile.PayeeFormat, ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);
            ShouldCreateNewPayeeIfNotExisting = new ReactivePropertySlim<bool>(
                profile.ShouldCreateNewPayeeIfNotExisting,
                ReactivePropertyMode.DistinctUntilChanged)
                .CompositeDisposalWith(_compositeDisposable);
            NewProfileName = new ReactiveProperty<string>("", ReactivePropertyMode.DistinctUntilChanged);
            MemoFormat = new ReactivePropertySlim<string>(profile.MemoFormat, ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);
            SumFormula = new ReactivePropertySlim<string>(profile.SumFormat, ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);

            SaveToProfile = new RxRelayCommand(() =>
            {
                profile.Header = Header.Value;
                profile.Delimiter = Delimiter.Value;
                profile.DateSegment = DateSegment.Value;
                profile.DateLocalization = DateLocalization.Value?.Name ?? String.Empty;
                profile.PayeeFormat = PayeeFormat.Value;
                profile.ShouldCreateNewPayeeIfNotExisting = ShouldCreateNewPayeeIfNotExisting.Value;
                profile.MemoFormat = MemoFormat.Value;
                profile.SumFormat = SumFormula.Value;
                profile.SumLocalization = SumLocalization.Value?.Name ?? String.Empty;
                profileManager.Save();
            }).CompositeDisposalWith(_compositeDisposable);
            
            SaveNewProfile = NewProfileName
                .Merge(profileManager.Profiles.CollectionChangedAsObservable().Select(_ => NewProfileName.Value))
                .Select(NewProfileCondition)
                .ToRxRelayCommand(() =>
                {
                    if (!NewProfileCondition(NewProfileName.Value)) return;
                    profile = createProfile.Create(
                        Header.Value,
                        Delimiter.Value,
                        DateSegment.Value,
                        DateLocalization.Value?.Name ?? String.Empty,
                        PayeeFormat.Value,
                        ShouldCreateNewPayeeIfNotExisting.Value,
                        MemoFormat.Value,
                        SumFormula.Value,
                        SumLocalization.Value?.Name ?? String.Empty,
                        NewProfileName.Value);
                    NewProfileName.Value = "";
                }, NewProfileCondition(NewProfileName.Value)).CompositeDisposalWith(_compositeDisposable);

            RemoveProfile = new RxRelayCommand(() =>
                profileManager.Remove(profile.Name)).CompositeDisposalWith(_compositeDisposable);

            ResetProfile = new RxRelayCommand(() =>
            {
                Header.Value = profile.Header;
                Delimiter.Value = profile.Delimiter;
                DateSegment.Value = profile.DateSegment;
                DateLocalization.Value = CultureInfo.GetCultureInfo(profile.DateLocalization);
                PayeeFormat.Value = profile.PayeeFormat;
                ShouldCreateNewPayeeIfNotExisting.Value = profile.ShouldCreateNewPayeeIfNotExisting;
                MemoFormat.Value = profile.MemoFormat;
                SumFormula.Value = profile.SumFormat;
                SumLocalization.Value = CultureInfo.GetCultureInfo(profile.SumLocalization);
            }).CompositeDisposalWith(_compositeDisposable);
        }

        public IReadOnlyReactiveProperty<string> Name { get; }
        public IReactiveProperty<string> Header { get; }
        public IReactiveProperty<char> Delimiter { get; }
        public IReactiveProperty<string?> DateSegment { get; }

        public IReactiveProperty<CultureInfo?> DateLocalization { get; }
        public IReactiveProperty<string> PayeeFormat { get; }
        public IReactiveProperty<bool> ShouldCreateNewPayeeIfNotExisting { get; }
        public IReactiveProperty<string> MemoFormat { get; }
        public IReactiveProperty<CultureInfo?> SumLocalization { get; }
        public IReactiveProperty<string> NewProfileName { get; }
        public IReadOnlyReactiveProperty<string[]> Segments { get; }
        public IReactiveProperty<string> SumFormula { get; }
        public IRxRelayCommand SaveToProfile { get; }
        public IRxRelayCommand RemoveProfile { get; }
        public IRxRelayCommand SaveNewProfile { get; }
        public IRxRelayCommand ResetProfile { get; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }

    internal class CsvBankStatementImportNonProfileViewModel : ViewModelBase, ICsvBankStatementImportNonProfileViewModel
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
                return !name.IsWhitespace() && profileManager.Profiles.All(p => p.Name != name.Trim());
            }

            bool NotSameCountOrZero(IEnumerable<string> lines, char delimiter)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                int count = lines.First().Count(c => c == delimiter);

                if (count == 0) return true;

                // ReSharper disable once PossibleMultipleEnumeration
                return lines.Any(line => count != line.Count(c => c == delimiter));
            }
            bool SameCountAndNotZero(IEnumerable<string> lines, char delimiter)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                int count = lines.First().Count(c => c == delimiter);

                if (count == 0) return false;

                // ReSharper disable once PossibleMultipleEnumeration
                return lines.All(line => count == line.Count(c => c == delimiter));
            }

            Header = new ReactiveProperty<string>(filePath.Select(path => path is not null && File.Exists(path) ? File.ReadLines(path, Encoding.Default).FirstOrDefault() : ""),
                mode: ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);
            Delimiter = new ReactiveProperty<char>(
                Header
                    .Where(header => !string.IsNullOrWhiteSpace(header))
                    .Where(_ => NotSameCountOrZero(File.ReadLines(filePath.Value, Encoding.Default).Take(5), Delimiter.Value))
                    .Select(_ => new[] { ',', ';', '\t' }
                        .FirstOrDefault(
                            delimiter => SameCountAndNotZero(
                                File.ReadLines(filePath.Value, Encoding.Default).Take(5),
                                delimiter))), schedulerProvider.UI, mode: ReactivePropertyMode.DistinctUntilChanged);

            Segments = Header.Select(_ => Unit.Default)
                .Merge(Delimiter.Select(_ => Unit.Default))
                .Select(_ => Header.Value.Split(Delimiter.Value))
                .ToReadOnlyReactiveProperty();
            DateSegment = Segments.Select(s => s?.FirstOrDefault()).ToReactiveProperty();

            DateLocalization = new ReactivePropertySlim<CultureInfo?>(CultureInfo.InvariantCulture, ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);
            SumLocalization = new ReactivePropertySlim<CultureInfo?>(CultureInfo.InvariantCulture, ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);
            PayeeFormat = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);
            ShouldCreateNewPayeeIfNotExisting = new ReactivePropertySlim<bool>(
                    true,
                    ReactivePropertyMode.DistinctUntilChanged)
                .CompositeDisposalWith(_compositeDisposable);
            NewProfileName = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged);
            MemoFormat = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);
            SumFormula = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);

            SaveNewProfile = NewProfileName
                .Merge(profileManager.Profiles.CollectionChangedAsObservable().Select(_ => NewProfileName.Value))
                .Select(NewProfileCondition)
                .ToRxRelayCommand(() =>
                {
                    if (!NewProfileCondition(NewProfileName.Value)) return;
                    createProfile.Create(
                        Header.Value,
                        Delimiter.Value,
                        DateSegment.Value,
                        DateLocalization.Value?.Name ?? String.Empty,
                        PayeeFormat.Value,
                        ShouldCreateNewPayeeIfNotExisting.Value,
                        MemoFormat.Value,
                        SumFormula.Value,
                        SumLocalization.Value?.Name ?? String.Empty,
                        NewProfileName.Value);
                    NewProfileName.Value = "";
                }, NewProfileCondition(NewProfileName.Value)).CompositeDisposalWith(_compositeDisposable);

            ResetProfile = new RxRelayCommand(() =>
            {
                Header.Value = "";
                Delimiter.Value = ',';
                DateSegment.Value = "";
                DateLocalization.Value = CultureInfo.InvariantCulture;
                PayeeFormat.Value = "";
                ShouldCreateNewPayeeIfNotExisting.Value = false;
                MemoFormat.Value = "";
                SumFormula.Value = "";
                SumLocalization.Value = CultureInfo.InvariantCulture;
            }).CompositeDisposalWith(_compositeDisposable);
        }
        public IReactiveProperty<string> Header { get; }
        public IReactiveProperty<char> Delimiter { get; }
        public IReactiveProperty<string?> DateSegment { get; }
        public IReactiveProperty<CultureInfo?> DateLocalization { get; }
        public IReactiveProperty<string> PayeeFormat { get; }
        public IReactiveProperty<bool> ShouldCreateNewPayeeIfNotExisting { get; }
        public IReactiveProperty<string> MemoFormat { get; }
        public IReactiveProperty<CultureInfo?> SumLocalization { get; }
        public IReactiveProperty<string> NewProfileName { get; }
        public IReadOnlyReactiveProperty<string[]?> Segments { get; }
        public IReactiveProperty<string> SumFormula { get; }
        public IRxRelayCommand SaveNewProfile { get; }
        public IRxRelayCommand ResetProfile { get; }
        public IRxRelayCommand? SaveToProfile { get; } = null;
        public IRxRelayCommand? RemoveProfile { get; } = null;

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
