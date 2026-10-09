<template>
  <div>
    <splitpanes class="default-theme" style="height: calc(100vh - 80px);">
      <pane min-size="20" size="30">
        <ProjectTemplatesPanel @focused-template-changed="onFocusedTemplateChanged" />
      </pane>
      <pane min-size="20">
        <div>
          <h3 style="display: inline-block;">{{ templateName || 'No template selected' }}</h3>
        </div>

        <div class="se-template-items-data-grid">
          <div style="margin-bottom: 4px;">
            <Button class="se-add-row-button" @click="addNewItem">+</Button>
          </div>
          <DataTable
            v-if="templateId != null"
            :value="items"
            :pt="{ bodyRow: { class: 'se-data-row' } }"
          >
            <Column field="name">
              <template #body="{ data }">
                <template v-if="data._isNew">
                  <div class="se-text-box">
                    <InputText
                      ref="newItemInput"
                      class="se-text-editor-input"
                      v-model="data.name"
                      @keydown.enter.stop="saveNewItem(data)"
                      @keydown.escape.stop="cancelNewItem(data)"
                      @click.stop
                      style="width: 100%;"
                    />
                  </div>
                </template>
                <template v-else-if="editingItemId === data.id">
                  <div class="se-text-box">
                    <InputText
                      class="se-text-editor-input"
                      v-model="editingName"
                      @keydown.enter.stop="saveItemEdit(data)"
                      @keydown.escape.stop="cancelItemEdit"
                      @click.stop
                      style="width: 100%;"
                    />
                  </div>
                </template>
                <template v-else>
                  <div @click.stop="startItemEdit(data)" style="cursor: pointer; min-height: 1.5rem; width: 100%;">{{ data.name }}</div>
                </template>
              </template>
            </Column>
            <Column style="width: 40px;">
              <template #body="{ data }">
                <Button
                  v-if="!data._isNew"
                  class="se-delete-item-button"
                  icon="pi pi-trash"
                  text
                  rounded
                  severity="danger"
                  @click.stop="deleteItem(data)"
                />
              </template>
            </Column>
          </DataTable>
          <p v-else style="color: #888; padding: 8px 0;">Select a template to manage its items.</p>
        </div>
      </pane>
    </splitpanes>
  </div>
</template>

<script>
/* eslint-disable vue/no-reserved-component-names */
import { Pane, Splitpanes } from 'splitpanes';
import 'splitpanes/dist/splitpanes.css';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import ProjectTemplatesPanel from './ProjectTemplates/ProjectTemplatesPanel.vue';
import * as fetchUtils from '@/utils/fetchUtils.js';
import * as notifyUtils from '@/utils/notifyUtils';

export default {
  name: 'ProjectTemplatePage',
  components: { Splitpanes, Pane, DataTable, Column, InputText, Button, ProjectTemplatesPanel },
  data() {
    return {
      templateId: null,
      templateName: null,
      items: [],
      itemsUri: 'api/ProjectTemplateTasks',
      editingItemId: null,
      editingName: '',
    };
  },
  methods: {
    onFocusedTemplateChanged(template) {
      this.templateId = template?.id ?? null;
      this.templateName = template?.name ?? null;
      this.refreshItems();
    },
    addNewItem() {
      if (this.templateId == null) return;
      const newItem = { id: null, name: '', _isNew: true };
      this.items.push(newItem);
      this.$nextTick(() => {
        const input = this.$refs.newItemInput;
        if (input) {
          const el = Array.isArray(input) ? input[input.length - 1] : input;
          const domEl = el.$el || el;
          if (domEl && domEl.focus) domEl.focus();
        }
      });
    },
    async saveNewItem(item) {
      if (!item.name.trim()) {
        this.cancelNewItem(item);
        return;
      }
      try {
        await fetchUtils.post(this.itemsUri, {
          projectTemplateId: this.templateId,
          name: item.name.trim(),
        });
        this.refreshItems();
      } catch (error) {
        notifyUtils.notifySystemError('Unable to add item.', error);
      }
    },
    cancelNewItem(item) {
      const idx = this.items.indexOf(item);
      if (idx !== -1) this.items.splice(idx, 1);
    },
    startItemEdit(item) {
      this.editingItemId = item.id;
      this.editingName = item.name;
    },
    async saveItemEdit(item) {
      if (!this.editingName.trim()) {
        this.cancelItemEdit();
        return;
      }
      const name = this.editingName.trim();
      this.editingItemId = null;
      this.editingName = '';
      try {
        await fetchUtils.patch(this.itemsUri, [{ key: item.id, data: { name } }]);
        this.refreshItems();
      } catch (error) {
        notifyUtils.notifySystemError('Unable to update item.', error);
      }
    },
    cancelItemEdit() {
      this.editingItemId = null;
      this.editingName = '';
    },
    async deleteItem(item) {
      try {
        await fetchUtils.patch(this.itemsUri, [{ type: 'remove', key: item.id }]);
        this.refreshItems();
      } catch (error) {
        notifyUtils.notifySystemError('Unable to delete item.', error);
      }
    },
    refreshItems() {
      if (this.templateId == null) {
        this.items = [];
        return;
      }
      fetchUtils.get(`${this.itemsUri}?projectTemplateId=${this.templateId}`).then(data => {
        if (data) {
          this.items = data.map(i => ({ id: i.id, name: i.name }));
          this.editingItemId = null;
          this.editingName = '';
        }
      });
    },
  },
};
</script>
