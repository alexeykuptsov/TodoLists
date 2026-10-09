<template>
  <div>
    <h3>Project Templates</h3>
    <div class="se-project-templates-data-grid">
      <div style="margin-bottom: 8px;">
        <Button class="se-add-row-button" label="+" @click="addNewTemplate" />
      </div>
      <table style="width: 100%; border-collapse: collapse;">
        <tbody>
          <tr
            v-for="template in templates"
            :key="template.id ?? 'new'"
            class="se-data-row"
            :class="{ 'se-focused-row': focusedTemplate && focusedTemplate.id === template.id }"
            @click="selectTemplate(template)"
            style="border-bottom: 1px solid #eee;"
          >
            <td style="padding: 4px;">
              <template v-if="editingTemplateId !== null && editingTemplateId === template.id">
                <div class="se-text-box">
                  <InputText
                    ref="editInput"
                    class="se-text-editor-input"
                    v-model="editingName"
                    @keydown.enter.stop="saveEdit(template)"
                    @keydown.escape.stop="cancelEdit"
                    @click.stop
                    style="width: 100%;"
                  />
                </div>
              </template>
              <template v-else-if="template._isNew">
                <div class="se-text-box">
                  <InputText
                    ref="newTemplateInput"
                    class="se-text-editor-input"
                    v-model="newTemplateName"
                    @keydown.enter.stop="saveNewTemplate"
                    @keydown.escape.stop="cancelNewTemplate"
                    @click.stop
                    style="width: 100%;"
                  />
                </div>
              </template>
              <template v-else>{{ template.name }}</template>
            </td>
            <td style="padding: 4px; white-space: nowrap; width: 56px;">
              <template v-if="editingTemplateId !== null && editingTemplateId === template.id">
                <Button icon="pi pi-check" text rounded @click.stop="saveEdit(template)" />
                <Button icon="pi pi-times" text rounded @click.stop="cancelEdit" />
              </template>
              <template v-else-if="!template._isNew">
                <Button class="se-edit-button" icon="pi pi-pencil" text rounded @click.stop="startEdit(template)" />
                <Button class="se-delete-button" icon="pi pi-trash" text rounded severity="danger" @click.stop="confirmDelete(template)" />
              </template>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <Dialog
      v-model:visible="deleteDialogVisible"
      :modal="true"
      header="Delete Confirmation"
      :pt="{ root: { class: 'se-dialog' }, content: { class: 'se-dialog-content' } }"
      @hide="templateToDelete = null"
    >
      <p class="se-dialog-message">Do you really want to delete template "{{ templateToDelete?.name }}"?</p>
      <template #footer>
        <Button class="se-confirm-yes-button" label="Yes" @click="executeDelete" />
        <Button label="No" @click="deleteDialogVisible = false" />
      </template>
    </Dialog>
  </div>
</template>

<script>
/* eslint-disable vue/no-reserved-component-names */
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import Dialog from 'primevue/dialog';
import * as fetchUtils from '@/utils/fetchUtils.js';
import * as notifyUtils from '@/utils/notifyUtils';

export default {
  name: 'ProjectTemplatesPanel',
  emits: ['focused-template-changed'],
  components: { InputText, Button, Dialog },
  data() {
    return {
      templates: [],
      templatesUri: 'api/ProjectTemplates',
      focusedTemplate: null,
      editingTemplateId: null,
      editingName: '',
      newTemplateName: '',
      deleteDialogVisible: false,
      templateToDelete: null,
    };
  },
  mounted() {
    this.$nextTick(function () {
      this.refreshData();
    });
  },
  methods: {
    async refreshData() {
      const data = await fetchUtils.get(this.templatesUri);
      if (data) this._displayTemplates(data);
    },
    _displayTemplates(data) {
      const previousFocusedId = this.focusedTemplate?.id;

      this.templates = data.map(item => ({ id: item.id, name: item.name }));
      this.editingTemplateId = null;
      this.editingName = '';
      this.newTemplateName = '';

      const templateToFocus =
        this.templates.find(t => t.id === previousFocusedId) || this.templates[0];

      if (templateToFocus) {
        this.focusedTemplate = templateToFocus;
        this.$emit('focused-template-changed', templateToFocus);
      } else {
        this.focusedTemplate = null;
        this.$emit('focused-template-changed', null);
      }
    },
    selectTemplate(template) {
      if (template._isNew || this.editingTemplateId === template.id) return;
      this.focusedTemplate = template;
      this.$emit('focused-template-changed', template);
    },
    addNewTemplate() {
      this.newTemplateName = '';
      const newTemplate = { id: null, name: '', _isNew: true };
      this.templates.push(newTemplate);
      this.$nextTick(() => {
        const input = this.$refs.newTemplateInput;
        if (input) {
          const el = Array.isArray(input) ? input[input.length - 1] : input;
          const domEl = el.$el || el;
          if (domEl && domEl.focus) domEl.focus();
        }
      });
    },
    async saveNewTemplate() {
      if (!this.newTemplateName.trim()) {
        this.cancelNewTemplate();
        return;
      }
      const name = this.newTemplateName.trim();
      this.newTemplateName = '';
      try {
        await fetchUtils.post(this.templatesUri, { name });
        this.refreshData();
      } catch (error) {
        notifyUtils.notifySystemError('Unable to add template.', error);
      }
    },
    cancelNewTemplate() {
      const idx = this.templates.findIndex(t => t._isNew);
      if (idx !== -1) this.templates.splice(idx, 1);
      this.newTemplateName = '';
    },
    startEdit(template) {
      this.editingTemplateId = template.id;
      this.editingName = template.name;
      this.$nextTick(() => {
        const input = this.$refs.editInput;
        if (input) {
          const el = Array.isArray(input) ? input[0] : input;
          const domEl = el.$el || el;
          if (domEl && domEl.focus) domEl.focus();
        }
      });
    },
    async saveEdit(template) {
      if (!this.editingName.trim()) {
        this.cancelEdit();
        return;
      }
      const name = this.editingName.trim();
      this.editingTemplateId = null;
      this.editingName = '';
      try {
        await fetchUtils.patch(this.templatesUri, [{ type: 'update', data: { id: template.id, name } }]);
        this.refreshData();
      } catch (error) {
        notifyUtils.notifySystemError('Unable to update template.', error);
      }
    },
    cancelEdit() {
      this.editingTemplateId = null;
      this.editingName = '';
    },
    confirmDelete(template) {
      this.templateToDelete = template;
      this.deleteDialogVisible = true;
    },
    async executeDelete() {
      if (!this.templateToDelete) return;
      const template = this.templateToDelete;
      this.deleteDialogVisible = false;
      this.templateToDelete = null;
      try {
        await fetchUtils.patch(this.templatesUri, [{ type: 'remove', key: template.id }]);
        this.refreshData();
      } catch (error) {
        notifyUtils.notifySystemError('Unable to delete template.', error);
      }
    },
  },
};
</script>

<style scoped>
.se-focused-row td {
  background-color: #e8f4f8;
}
</style>
