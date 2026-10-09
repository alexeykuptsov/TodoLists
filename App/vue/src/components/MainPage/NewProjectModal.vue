<template>
  <Dialog
    v-model:visible="dialogVisible"
    :modal="true"
    header="New project"
    :pt="{ root: { class: 'se-new-project-dialog' } }"
  >
    <div style="display: flex; flex-direction: column; gap: 12px; min-width: 300px;">
      <div>
        <label style="display: block; margin-bottom: 4px;">Project name</label>
        <InputText
          class="se-new-project-name-input"
          v-model="projectName"
          @keydown.enter.stop="submit"
          @keydown.escape.stop="cancel"
          style="width: 100%;"
          placeholder="Enter project name"
        />
      </div>
      <div>
        <label style="display: block; margin-bottom: 4px;">Template</label>
        <Select
          class="se-template-select"
          v-model="selectedTemplateId"
          :options="templateOptions"
          optionLabel="label"
          optionValue="value"
          style="width: 100%;"
        />
      </div>
    </div>
    <template #footer>
      <Button class="se-new-project-submit-button" label="Create" @click="submit" />
      <Button label="Cancel" severity="secondary" @click="cancel" />
    </template>
  </Dialog>
</template>

<script>
/* eslint-disable vue/no-reserved-component-names */
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import Select from 'primevue/select';
import Button from 'primevue/button';
import * as fetchUtils from '@/utils/fetchUtils.js';
import * as notifyUtils from '@/utils/notifyUtils';

export default {
  name: 'NewProjectModal',
  components: { Dialog, InputText, Select, Button },
  props: {
    visible: {
      type: Boolean,
      default: false,
    },
  },
  emits: ['update:visible', 'project-created'],
  data() {
    return {
      templates: [],
      selectedTemplateId: '',
      projectName: '',
    };
  },
  computed: {
    dialogVisible: {
      get() { return this.visible; },
      set(val) { this.$emit('update:visible', val); },
    },
    templateOptions() {
      return [
        { label: 'Empty', value: '' },
        ...this.templates.map(t => ({ label: t.name, value: t.id })),
      ];
    },
  },
  async mounted() {
    const data = await fetchUtils.get('api/ProjectTemplates');
    if (data) this.templates = data;
  },
  methods: {
    async submit() {
      const name = this.projectName.trim();
      if (!name) return;
      try {
        if (this.selectedTemplateId) {
          await fetchUtils.post('api/Projects/FromTemplate', { name, templateId: this.selectedTemplateId });
        } else {
          await fetchUtils.post('api/Projects', { name });
        }
        this.projectName = '';
        this.selectedTemplateId = '';
        this.$emit('update:visible', false);
        this.$emit('project-created');
      } catch (error) {
        notifyUtils.notifySystemError('Unable to create project.', error);
      }
    },
    cancel() {
      this.projectName = '';
      this.selectedTemplateId = '';
      this.$emit('update:visible', false);
    },
  },
};
</script>
