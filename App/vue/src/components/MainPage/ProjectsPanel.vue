<script>
import {DxColumn, DxDataGrid, DxEditing, DxRowDragging, DxToolbar, DxItem} from "devextreme-vue/data-grid";
import {DxButton} from "devextreme-vue/button";
import * as fetchUtils from '@/utils/fetchUtils.js';
import * as notifyUtils from "@/utils/notifyUtils";
import {confirm} from "devextreme/ui/dialog";

const projectsDataGridRefKey = 'projects-data-grid';

export default {
  name: 'ProjectsPanel',
  emits: [ 'focused-project-changed' ],
  components: {
    DxDataGrid,
    DxColumn,
    DxEditing,
    DxRowDragging,
    DxToolbar,
    DxItem,
    DxButton,
  },
  data() {
    return {
      projects: [],
      projectsDataGridRefKey,
      projectsUri: 'api/Projects',
      projectsDataGridFocusedRowIndex: -1,
      newRowPosition: "last",
      focusedRow: null,
    };
  },
  computed: {
    projectsDataGrid: function() {
      return this.$refs[projectsDataGridRefKey].instance;
    },
  },
  mounted() {
    this.$nextTick(function () {
      this.refreshData();
    });
  },
  methods: {
    refreshData() {
      fetchUtils.get(this.projectsUri).then(data => {
        this._displayProjects(data);
      });
    },
    projectsDataGrid_onSaved(e) {
      fetchUtils.patch(this.projectsUri, e.changes)
        .then(() => {
          this.refreshData()
        })
        .catch(error => notifyUtils.notifySystemError('Unable to patch item.', error));
    },
    projectsDataGrid_onRowRemoving(e) {
      if (this.projects.length === 1)
      {
        notifyUtils.notifyValidationError(
          'It is impossible to delete the last project. There should be at least one.', 'Error');
        e.cancel = true;
        return;
      }

      let index = e.component.getRowIndexByKey(e.key);
      let rowEl = e.component.getRowElement(index);

      let res = confirm("Do you really want to delete a record with key:" + e.key, "Warning");

      e.cancel = new Promise((resolve, reject) => {
        res.then((dialogResult) => {
          resolve(!dialogResult)
        });
      })
    },
    _displayProjects(data) {
      this.projects.splice(0);
      data.forEach(item => {
        this.projects.push({
          id: item.id,
          name: item.name,
        });
      });
      // this.projectsDataSource.store().load();
      this.projectsDataGrid.refresh()
        .done(() => {
          this.projectsDataGridFocusedRowIndex = 0;
        });
    },
    onFocusedRowChanged(e) {
      this.focusedRow = e.row.data;
      this.$emit('focused-project-changed', e)
    },
    cloneProject() {
      fetchUtils.post('api/Projects/clone', { id: this.focusedRow.id }).then(() => {
        this.refreshData();
      });
    },
  }
}
</script>

<template>
  <h3>Projects</h3>

  <DxDataGrid
    :class="{ 'se-projects-data-grid': true }"
    :ref="projectsDataGridRefKey"
    :data-source="projects"
    :remote-operations="false"
    :allow-column-reordering="true"
    :row-alternation-enabled="true"
    :show-borders="true"
    :show-column-headers="false"
    :focused-row-enabled="true"
    :focused-row-index="projectsDataGridFocusedRowIndex"
    :auto-navigate-to-focused-row="true"
    :new-row-position="newRowPosition"
    @focused-row-changed="onFocusedRowChanged"
    @saved="projectsDataGrid_onSaved"
    @row-removing="projectsDataGrid_onRowRemoving"
  >
    <DxEditing
      :allow-updating="true"
      :allow-deleting="true"
      :allow-adding="true"
      mode="row"
      :confirm-delete="false"
    />
    <DxRowDragging
      :allow-reordering="true"
    />
    <DxColumn data-field="name"/>
    <DxToolbar>
      <DxItem
        location="after"
        template="cloneButton"
      />
      <DxItem
        location="after"
        name="addRowButton"
      />
    </DxToolbar>
    <template #cloneButton>
      <DxButton
        :class="{ 'se-clone-button': true }"
        icon="copy"
        @click="cloneProject"
      />
    </template>
  </DxDataGrid>
</template>

<style scoped>

</style>