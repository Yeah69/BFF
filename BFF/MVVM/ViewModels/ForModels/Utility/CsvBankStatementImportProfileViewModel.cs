﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
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
        IReactiveProperty<string> DateSegment { get; }
        IReactiveProperty<CultureInfo> DateLocalization { get; }
        IReactiveProperty<string> PayeeFormat { get; }
        IReactiveProperty<bool> ShouldCreateNewPayeeIfNotExisting { get; }
        IReactiveProperty<string> MemoFormat { get; }
        IReactiveProperty<string> SumFormula { get; }
        IReactiveProperty<CultureInfo> SumLocalization { get; }
        IReactiveProperty<string> NewProfileName { get; }

        IReadOnlyReactiveProperty<string[]> Segments { get; }

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

            Segments = Header.Select(_ => Unit.Default)
                .Merge(Delimiter.Select(_ => Unit.Default))
                .Select(_ => Header.Value.Split(Delimiter.Value))
                .ToReadOnlyReactiveProperty(Header.Value.Split(Delimiter.Value));
            DateSegment = Segments.Select(s => s?.FirstOrDefault(seg => seg == profile.DateSegment) ?? s?.FirstOrDefault()).ToReactiveProperty();

            DateLocalization = profile
                .ObserveProperty(p => p.DateLocalization)
                .Select(CultureInfo.GetCultureInfo)
                .ToReactiveProperty(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddHere(_compositeDisposable);
            SumLocalization = profile
                .ObserveProperty(p => p.SumLocalization)
                .Select(CultureInfo.GetCultureInfo)
                .ToReactiveProperty(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddHere(_compositeDisposable);

            PayeeFormat = new ReactivePropertySlim<string>(profile.PayeeFormat, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            ShouldCreateNewPayeeIfNotExisting = new ReactivePropertySlim<bool>(
                profile.ShouldCreateNewPayeeIfNotExisting,
                ReactivePropertyMode.DistinctUntilChanged)
                .AddHere(_compositeDisposable);
            NewProfileName = new ReactiveProperty<string>("", ReactivePropertyMode.DistinctUntilChanged);
            MemoFormat = new ReactivePropertySlim<string>(profile.MemoFormat, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            SumFormula = new ReactivePropertySlim<string>(profile.SumFormat, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

            SaveToProfile = new ReactiveCommand().AddHere(_compositeDisposable);
            SaveToProfile.Subscribe(_ =>
            {
                profile.Header = Header.Value;
                profile.Delimiter = Delimiter.Value;
                profile.DateSegment = DateSegment.Value;
                profile.DateLocalization = DateLocalization.Value.Name;
                profile.PayeeFormat = PayeeFormat.Value;
                profile.ShouldCreateNewPayeeIfNotExisting = ShouldCreateNewPayeeIfNotExisting.Value;
                profile.MemoFormat = MemoFormat.Value;
                profile.SumFormat = SumFormula.Value;
                profile.SumLocalization = SumLocalization.Value.Name;
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
                    DateSegment.Value,
                    DateLocalization.Value.Name,
                    PayeeFormat.Value,
                    ShouldCreateNewPayeeIfNotExisting.Value,
                    MemoFormat.Value,
                    SumFormula.Value,
                    SumLocalization.Value.Name,
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
                DateSegment.Value = profile.DateSegment;
                DateLocalization.Value = CultureInfo.GetCultureInfo(profile.DateLocalization);
                PayeeFormat.Value = profile.PayeeFormat;
                ShouldCreateNewPayeeIfNotExisting.Value = profile.ShouldCreateNewPayeeIfNotExisting;
                MemoFormat.Value = profile.MemoFormat;
                SumFormula.Value = profile.SumFormat;
                SumLocalization.Value = CultureInfo.GetCultureInfo(profile.SumLocalization);
            }).AddHere(_compositeDisposable);
        }

        public IReadOnlyReactiveProperty<string> Name { get; }
        public IReactiveProperty<string> Header { get; }
        public IReactiveProperty<char> Delimiter { get; }
        public IReactiveProperty<string> DateSegment { get; }

        public IReactiveProperty<CultureInfo> DateLocalization { get; }
        public IReactiveProperty<string> PayeeFormat { get; }
        public IReactiveProperty<bool> ShouldCreateNewPayeeIfNotExisting { get; }
        public IReactiveProperty<string> MemoFormat { get; }
        public IReactiveProperty<CultureInfo> SumLocalization { get; }
        public IReactiveProperty<string> NewProfileName { get; }
        public IReadOnlyReactiveProperty<string[]> Segments { get; }
        public IReactiveProperty<string> SumFormula { get; }
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

            Segments = Header.Select(_ => Unit.Default)
                .Merge(Delimiter.Select(_ => Unit.Default))
                .Select(_ => Header.Value.Split(Delimiter.Value))
                .ToReadOnlyReactiveProperty();
            DateSegment = Segments.Select(s => s?.FirstOrDefault()).ToReactiveProperty();

            DateLocalization = new ReactivePropertySlim<CultureInfo>(CultureInfo.InvariantCulture, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            SumLocalization = new ReactivePropertySlim<CultureInfo>(CultureInfo.InvariantCulture, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            PayeeFormat = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            ShouldCreateNewPayeeIfNotExisting = new ReactivePropertySlim<bool>(
                    true,
                    ReactivePropertyMode.DistinctUntilChanged)
                .AddHere(_compositeDisposable);
            NewProfileName = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged);
            MemoFormat = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            SumFormula = new ReactivePropertySlim<string>("", ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

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
                    DateSegment.Value,
                    DateLocalization.Value.Name,
                    PayeeFormat.Value,
                    ShouldCreateNewPayeeIfNotExisting.Value,
                    MemoFormat.Value,
                    SumFormula.Value,
                    SumLocalization.Value.Name,
                    NewProfileName.Value);
                NewProfileName.Value = "";
            }).AddHere(_compositeDisposable);

            ResetProfile = new ReactiveCommand().AddHere(_compositeDisposable);
            ResetProfile.Subscribe(_ =>
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
            }).AddHere(_compositeDisposable);
        }
        public IReactiveProperty<string> Header { get; }
        public IReactiveProperty<char> Delimiter { get; }
        public IReactiveProperty<string> DateSegment { get; }
        public IReactiveProperty<CultureInfo> DateLocalization { get; }
        public IReactiveProperty<string> PayeeFormat { get; }
        public IReactiveProperty<bool> ShouldCreateNewPayeeIfNotExisting { get; }
        public IReactiveProperty<string> MemoFormat { get; }
        public IReactiveProperty<CultureInfo> SumLocalization { get; }
        public IReactiveProperty<string> NewProfileName { get; }
        public IReadOnlyReactiveProperty<string[]> Segments { get; }
        public IReactiveProperty<string> SumFormula { get; }
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
