// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using JSSoft.Crema.Runtime.Generation;
using JSSoft.Crema.Runtime.Serialization;
using JSSoft.Crema.RuntimeService;
using JSSoft.Crema.Tools.Framework;
using JSSoft.Crema.Tools.Framework.Dialogs.ViewModels;
using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.Dialogs.ViewModels;

/* 'JSSoft.Crema.Tools.Runtime (net452)' 프로젝트에서 병합되지 않은 변경 내용
이전:
using System;
이후:
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
*/
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Tools.Runtime.ViewModels
{
    [Export(typeof(IContent))]
    class GenerationViewModel : ContentBase
    {
        private readonly IRuntimeService service;
        private readonly IEnumerable<ICodeGenerator> generators;
        private readonly IEnumerable<IDataSerializer> serializers;
        private readonly IAppConfiguration configs;
        private readonly string[] languageTypes;
        private readonly GenerationItemCollection settingsList;
        private GenerationItemViewModel selectedItem;
        private GenerationItemViewModel settings = new();
        
        private bool openAfterGenerate;

        [ImportingConstructor]
        public GenerationViewModel(IRuntimeService service, [ImportMany]IEnumerable<ICodeGenerator> generators, [ImportMany]IEnumerable<IDataSerializer> serializers, IAppConfiguration configs)
        {
            this.GroupName = "Runtime";
            this.DisplayName = string.Empty;
            this.service = service;
            this.configs = configs;
            this.generators = generators;
            this.serializers = serializers;
            this.languageTypes = generators.Select(item => item.Name).ToArray();
            this.settingsList = new GenerationItemCollection(configs);
            this.SelectedItem = this.settingsList.FirstOrDefault();
            this.configs.Update(this);
        }

        public void SelectPath()
        {
            var dialog = new CommonOpenFileDialog() { IsFolderPicker = true, };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.OutputPath = dialog.FileName;
            }
        }

        public async Task SelectDataBaseAsync()
        {
            var dialog = new DataBaseListViewModel(this.Address);
            if (await dialog.ShowDialogAsync() == true)
            {
                this.DataBase = dialog.SelectedItem.Value.Name;
            }
        }

        public async Task GenerateAsync()
        {
            try
            {
                this.BeginProgress();
                var metaData = await this.service.GetMetaDataAsync(this.Address, this.DataBase, this.Tags, this.FilterExpression, null);
                var generator = this.generators.FirstOrDefault(item => item.Name == this.LanguageType);
                generator.Generate(this.OutputPath, metaData.Item1, CodeGenerationSettings.Default);
                var serializer = this.serializers.First();
                serializer.Serialize(System.IO.Path.Combine(this.OutputPath, "crema.dat"), metaData.Item2);

                this.settingsList.Add(this.settings);
                this.configs.Commit(this);
                if (this.OpenAfterGenerate == true)
                {
                    Process.Start("explorer", this.OutputPath);
                }
                this.EndProgress();
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
                this.EndProgress();
            }
        }

        public async Task EditFilterExpressionAsync()
        {
            var dialog = new EditFilterExpressionViewModel()
            {
                FilterExpression = this.FilterExpression,
            };
            if (await dialog.ShowDialogAsync() == true)
            {
                this.FilterExpression = dialog.FilterExpression;
            }
        }

        public string SettingsName
        {
            get => this.settings.Name;
            set
            {
                this.settings.Name = value;
                this.NotifyOfPropertyChange(() => this.SettingsName);
                this.NotifyOfPropertyChange(() => this.CanGenerate);
            }
        }

        public string Address
        {
            get => this.settings.Address;
            set
            {
                this.settings.Address = value;
                this.NotifyOfPropertyChange(() => this.Address);
                this.NotifyOfPropertyChange(() => this.CanGenerate);
            }
        }

        public string DataBase
        {
            get => this.settings.DataBase;
            set
            {
                this.settings.DataBase = value;
                this.NotifyOfPropertyChange(() => this.DataBase);
                this.NotifyOfPropertyChange(() => this.CanGenerate);
            }
        }

        public string Tags
        {
            get => this.settings.Tags;
            set
            {
                this.settings.Tags = value;
                this.NotifyOfPropertyChange(() => this.Tags);
                this.NotifyOfPropertyChange(() => this.CanGenerate);
            }
        }

        public string OutputPath
        {
            get => this.settings.OutputPath;
            set
            {
                this.settings.OutputPath = value;
                this.NotifyOfPropertyChange(() => this.OutputPath);
                this.NotifyOfPropertyChange(() => this.CanGenerate);
            }
        }


/* 'JSSoft.Crema.Tools.Runtime (net452)' 프로젝트에서 병합되지 않은 변경 내용
이전:
        public IEnumerable<string> LanguageTypes
        {
            get { return this.languageTypes; }
이후:
        public IEnumerable<string> LanguageTypes => this.languageTypes; }
*/
        public IEnumerable<string> LanguageTypes => this.languageTypes;

        public string LanguageType
        {
            get => this.settings.LanguageType;
            set
            {
                this.settings.LanguageType = value;
                this.NotifyOfPropertyChange(() => this.LanguageType);
                this.NotifyOfPropertyChange(() => this.CanGenerate);
            }
        }

        [Obsolete]
        public bool IsDevmode
        {
            get => this.settings.IsDevmode;
            set
            {
                this.settings.IsDevmode = value;
                this.NotifyOfPropertyChange(nameof(this.IsDevmode));
                this.NotifyOfPropertyChange(() => this.CanGenerate);
            }
        }

        [ConfigurationProperty("openAfterGenerate")]
        public bool OpenAfterGenerate
        {
            get => this.openAfterGenerate;
            set
            {
                this.openAfterGenerate = value;
                this.NotifyOfPropertyChange(() => this.OpenAfterGenerate);
            }
        }

        public string FilterExpression
        {
            get => this.settings.FilterExpression ?? string.Empty;
            set
            {
                this.settings.FilterExpression = value;
                this.NotifyOfPropertyChange(() => this.FilterExpression);
            }
        }

        public bool CanGenerate
        {
            get
            {
                if (this.IsProgressing == true)
                    return false;
                if (this.languageTypes.Any(item => item == this.LanguageType) == false)
                    return false;
                if (this.Tags == string.Empty)
                    return false;
                if (this.Tags == TagInfo.Unused.ToString())
                    return false;
                if (this.DataBase == string.Empty)
                    return false;
                if (this.Address == string.Empty)
                    return false;
                if (DirectoryUtility.Exists(this.OutputPath) == false)
                    return false;
                return string.IsNullOrEmpty(this.OutputPath) == false;
            }
        }

        public IEnumerable<GenerationItemViewModel> ItemsSource => this.settingsList;

        public GenerationItemViewModel SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.selectedItem = value;
                if (this.selectedItem != null)
                    this.settings = value.Clone();
                else
                    this.settings = GenerationItemViewModel.Empty;
                this.Refresh();
            }
        }

        public event EventHandler Generated;

        protected virtual void OnGenerated(EventArgs e)
        {
            this.Generated?.Invoke(this, e);
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            this.configs.Update(this);
        }
    }
}
