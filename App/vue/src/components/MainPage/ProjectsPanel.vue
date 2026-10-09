<template>
  <div>
    <h3>Projects</h3>
    <div class="se-projects-data-grid">
      <div style="margin-bottom: 8px; display: flex; gap: 4px;">
        <Button class="se-clone-button" label="Clone" @click="cloneProject" />
        <Button class="se-add-row-button" label="New project" @click="openNewProjectModal" />
      </div>
      <table style="width: 100%; border-collapse: collapse;">
        <draggable
          v-model="projects"
          tag="tbody"
          item-key="id"
          handle=".se-drag-handle"
          :forceFallback="true"
          @end="onDragEnd"
        >
          <template #item="{ element: project }">
            <tr
              class="se-data-row"
              :class="{ 'se-focused-row': focusedProject && focusedProject.id === project.id }"
              @click="selectProject(project)"
              style="border-bottom: 1px solid #eee;"
            >
              <td style="width: 28px; padding: 4px;">
                <span
                  class="se-drag-handle"
                  style="cursor: grab; user-select: none; display: inline-block; width: 16px;"
                  @click.stop
                >&#8801;</span>
              </td>
              <td style="padding: 4px;">
                <template v-if="editingProjectId !== null && editingProjectId === project.id">
                  <div class="se-text-box">
                    <InputText
                      ref="editInput"
                      class="se-text-editor-input"
                      v-model="editingName"
                      @keydown.enter.stop="saveEdit(project)"
                      @keydown.escape.stop="cancelEdit"
                      @click.stop
                      style="width: 100%;"
                    />
                  </div>
                </template>
                <template v-else>{{ project.name }}</template>
              </td>
              <td style="padding: 4px; white-space: nowrap; width: 56px;">
                <template v-if="editingProjectId !== null && editingProjectId === project.id">
                  <Button icon="pi pi-check" text rounded @click.stop="saveEdit(project)" />
                  <Button icon="pi pi-times" text rounded @click.stop="cancelEdit" />
                </template>
                <template v-else>
                  <Button class="se-edit-button" icon="pi pi-pencil" text rounded @click.stop="startEdit(project)" />
                  <Button class="se-delete-button" icon="pi pi-trash" text rounded severity="danger" @click.stop="confirmDelete(project)" />
                </template>
              </td>
            </tr>
          </template>
        </draggable>
      </table>
    </div>

    <Dialog
      v-model:visible="deleteDialogVisible"
      :modal="true"
      header="Delete Confirmation"
      :pt="{ root: { class: 'se-dialog' }, content: { class: 'se-dialog-content' } }"
      @hide="projectToDelete = null"
    >
      <p class="se-dialog-message">Do you really want to delete project "{{ projectToDelete?.name }}"?</p>
      <template #footer>
        <Button class="se-confirm-yes-button" label="Yes" @click="executeDelete" />
        <Button label="No" @click="deleteDialogVisible = false" />
      </template>
    </Dialog>

    <NewProjectModal v-model:visible="showNewProjectModal" @project-created="refreshData" />
  </div>
</template>

<script>
/* eslint-disable vue/no-reserved-component-names */
import draggable from 'vuedraggable';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import Dialog from 'primevue/dialog';
import NewProjectModal from './NewProjectModal.vue';
import * as fetchUtils from '@/utils/fetchUtils.js';
import * as notifyUtils from "@/utils/notifyUtils";

export default {
  name: 'ProjectsPanel',
  emits: ['focused-project-changed'],
  components: { draggable, InputText, Button, Dialog, NewProjectModal },
  data() {
    return {
      projects: [],
      projectsUri: 'api/Projects',
      focusedProject: null,
      editingProjectId: null,
      editingName: '',
      deleteDialogVisible: false,
      projectToDelete: null,
      showNewProjectModal: false,
    };
  },
  mounted() {
    this.$nextTick(function () {
      this.refreshData();
    });
  },
  methods: {
    async refreshData() {
      const data = await fetchUtils.get(this.projectsUri);
      if (data) this._displayProjects(data);
    },
    _displayProjects(data) {
      const previousFocusedId = this.focusedProject?.id;

      this.projects = data
        .map(item => ({ id: item.id, name: item.name, order: item.order }))
        .sort((a, b) => a.order - b.order);

      this.editingProjectId = null;
      this.editingName = '';

      const projectToFocus =
        this.projects.find(p => p.id === previousFocusedId) || this.projects[0];

      if (projectToFocus) {
        this.focusedProject = projectToFocus;
        this.$emit('focused-project-changed', { row: { data: projectToFocus } });
      }
    },
    selectProject(project) {
      if (this.editingProjectId === project.id) return;
      this.focusedProject = project;
      this.$emit('focused-project-changed', { row: { data: project } });
    },
    openNewProjectModal() {
      this.showNewProjectModal = true;
    },
    startEdit(project) {
      this.editingProjectId = project.id;
      this.editingName = project.name;
      this.$nextTick(() => {
        const input = this.$refs.editInput;
        if (input) {
          const el = Array.isArray(input) ? input[0] : input;
          const domEl = el.$el || el;
          if (domEl && domEl.focus) domEl.focus();
        }
      });
    },
    async saveEdit(project) {
      if (!this.editingName.trim()) {
        this.cancelEdit();
        return;
      }
      const name = this.editingName.trim();
      this.editingProjectId = null;
      this.editingName = '';
      try {
        await fetchUtils.patch(this.projectsUri, [{ type: 'update', data: { id: project.id, name: name } }]);
        this.refreshData();
      } catch (error) {
        notifyUtils.notifySystemError('Unable to update project.', error);
      }
    },
    cancelEdit() {
      this.editingProjectId = null;
      this.editingName = '';
    },
    confirmDelete(project) {
      if (this.projects.length === 1) {
        notifyUtils.notifyValidationError(
          'It is impossible to delete the last project. There should be at least one.');
        return;
      }
      this.projectToDelete = project;
      this.deleteDialogVisible = true;
    },
    async executeDelete() {
      if (!this.projectToDelete) return;
      const project = this.projectToDelete;
      this.deleteDialogVisible = false;
      this.projectToDelete = null;
      try {
        await fetchUtils.patch(this.projectsUri, [{ type: 'remove', key: project.id }]);
        this.refreshData();
      } catch (error) {
        notifyUtils.notifySystemError('Unable to delete project.', error);
      }
    },
    async cloneProject() {
      if (!this.focusedProject) return;
      try {
        await fetchUtils.post('api/Projects/clone', { id: this.focusedProject.id });
        this.refreshData();
      } catch (error) {
        notifyUtils.notifySystemError('Unable to clone project.', error);
      }
    },
    async onDragEnd() {
      const reorderedProjectIds = this.projects.filter(p => p.id != null).map(p => p.id);
      try {
        await fetchUtils.post('api/Projects/Reorder', { projectIds: reorderedProjectIds });
        this.refreshData();
      } catch (error) {
        notifyUtils.notifySystemError('Unable to reorder projects.', error);
        this.refreshData();
      }
    },
  }
}
</script>

<style scoped>
.se-focused-row td {
  background-color: #e8f4f8;
}
.se-drag-handle:active {
  cursor: grabbing;
}
</style>
