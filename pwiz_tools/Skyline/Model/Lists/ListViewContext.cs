﻿/*
 * Original author: Nicholas Shulman <nicksh .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2018 University of Washington - Seattle, WA
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using pwiz.Common.DataBinding;
using pwiz.Common.DataBinding.Controls;
using pwiz.Skyline.Alerts;
using pwiz.Skyline.Model.Databinding;
using pwiz.Skyline.Util.Extensions;

namespace pwiz.Skyline.Model.Lists
{
    public class ListViewContext : SkylineViewContext, INewRowHandler
    {
        public const string LIST_ROWSOURCE_PREFIX = "pwiz.Skyline.Model.Lists.list_";
        public static ListViewContext CreateListViewContext(SkylineDataSchema dataSchema, string listName)
        {
            var listItemType = ListItemTypes.INSTANCE.GetListItemType(listName);
            var rootColumn = ColumnDescriptor.RootColumn(dataSchema, listItemType);
            var rowSourceConstructor = typeof(ListRowSource<>).MakeGenericType(listItemType)
                .GetConstructor(new[] { typeof(SkylineDataSchema) });
            var rowSource = (IRowSource)rowSourceConstructor.Invoke(new object[] { dataSchema });
            var rowSourceInfo = new RowSourceInfo(listItemType, rowSource, new[] {GetDefaultViewInfo(rootColumn)},
                LIST_ROWSOURCE_PREFIX + listName, listName);
            return new ListViewContext(dataSchema, listName, rowSourceInfo);
        }

        private ListViewContext(SkylineDataSchema schema, string listName, RowSourceInfo rowSource) 
            : base(schema, new[]{rowSource})
        {
            ListName = listName;
            ListItemType = ListItemTypes.INSTANCE.GetListItemType(listName);
        }

        public string ListName { get; private set; }
        public Type ListItemType { get; private set; }

        public BoundDataGridView BoundDataGridView { get; set; }

        public RowItem AddNewRow()
        {
            return new RowItem(ListItem.NewRecord(ListItemType));
        }

        public void CancelAddNew()
        {
        }

        public RowItem CommitAddNew(RowItem rowItem)
        {
            var listItem = rowItem.Value as ListItem;
            if (listItem == null)
            {
                return null;
            }
            var values = ((ListItem.NewRecordData) listItem.GetRecord()).UncommittedValues;
            if (values.Count == 0)
            {
                return null;
            }
            ListItemId? listItemId = null;
            SkylineDataSchema.ModifyDocument(EditDescription.Message(string.Format("Add new item to list '{0}'", ListName)), doc =>
            {
                var listData = doc.Settings.DataSettings.FindList(ListName);
                ListItemId newItemId;
                listData = listData.AddRow(((ListItem.NewRecordData)listItem.GetRecord()).UncommittedValues, out newItemId);
                listItemId = newItemId;
                return doc.ChangeSettings(
                    doc.Settings.ChangeDataSettings(doc.Settings.DataSettings.ReplaceList(listData)));
            });
            if (!listItemId.HasValue)
            {
                return null;
            }
            return new RowItem(ConstructListItem(listItemId.Value));
        }

        public bool IsNewRowEmpty(RowItem rowItem)
        {
            var listItem = rowItem.Value as ListItem;
            if (listItem == null)
            {
                return true;
            }
            var values = ((ListItem.NewRecordData)listItem.GetRecord()).UncommittedValues;
            if (values.Count == 0)
            {
                return true;
            }
            return false;
        }

        public bool ValidateNewRow(RowItem rowItem, out bool cancelRowEdit)
        {
            cancelRowEdit = false;
            var listItem = rowItem.Value as ListItem;
            if (listItem == null)
            {
                return true;
            }
            var values = ((ListItem.NewRecordData) listItem.GetRecord()).UncommittedValues;
            if (values.Count == 0)
            {
                return true;
            }

            try
            {
                ListItemId listItemId;
                var listData = SkylineDataSchema.Document.Settings.DataSettings.FindList(ListName);
                listData.AddRow(values, out listItemId);
                return true;
            }
            catch (Exception exception)
            {
                if (MultiButtonMsgDlg.Show(BoundDataGridView,
                        TextUtil.LineSeparate("The new row could not be added because of the following error:",
                            exception.Message, "Press OK to continue editing your row, or Cancel to throw away the new row.")
                            
                            , MultiButtonMsgDlg.BUTTON_OK) == DialogResult.Cancel)
                {
                    cancelRowEdit = true;
                }
                return false;
            }
        }

        private ListItem ConstructListItem(ListItemId listItemId)
        {
            var listData = ((SkylineDataSchema) DataSchema).Document.Settings.DataSettings.FindList(ListName);
            return ListItem.ExistingRecord(listData, listItemId);
        }

        public override void Delete()
        {
            var selectedItems = GetSelectedListItems(BoundDataGridView);
            if (selectedItems.Count == 0)
            {
                return;
            }
            string message = string.Format("Are you sure you want to delete the {0} selected items from the list '{1}'?", selectedItems.Count, ListName);
            if (MultiButtonMsgDlg.Show(BoundDataGridView, message, MultiButtonMsgDlg.BUTTON_OK) != DialogResult.OK)
            {
                return;
            }
            SkylineDataSchema.ModifyDocument(EditDescription.Message(string.Format("Delete from list '{0}'", ListName)),
                doc =>
                {
                    var listData = doc.Settings.DataSettings.FindList(ListName);
                    listData = listData.DeleteItems(selectedItems);
                    return doc.ChangeSettings(
                        doc.Settings.ChangeDataSettings(doc.Settings.DataSettings.ReplaceList(listData)));

                });
        }

        public override bool DeleteEnabled
        {
            get { return BoundDataGridView != null; }
        }

        private HashSet<ListItemId> GetSelectedListItems(BoundDataGridView dataGridView)
        {
            var listIds = new HashSet<ListItemId>();
            if (dataGridView == null)
            {
                return listIds;
            }
            var bindingSource = dataGridView.DataSource as BindingListSource;
            if (bindingSource == null)
            {
                return listIds;
            }
            var selectedRows = dataGridView.SelectedRows.Cast<DataGridViewRow>()
                .Select(row => (RowItem)bindingSource[row.Index]).ToArray();
            if (!selectedRows.Any())
            {
                selectedRows = new[] { bindingSource.Current as RowItem };
            }
            foreach (var rowItem in selectedRows)
            {
                if (rowItem == null)
                {
                    continue;
                }
                var listItem = rowItem.Value as ListItem;
                if (listItem == null)
                {
                    continue;
                }
                var existingRecord = listItem.GetRecord() as ListItem.ExistingRecordData;
                if (null != existingRecord)
                {
                    listIds.Add(existingRecord.ListItemId);
                }
            }
            return listIds;
        }
    }
}
