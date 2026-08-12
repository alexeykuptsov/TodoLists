let _toast = null;

export function setToast(toast) {
  _toast = toast;
}

export function notifySystemError(message, error) {
  if (_toast) {
    _toast.add({ severity: 'error', summary: message, life: 5000 });
  }
  console.error(error != null ? message + '\n' + (error.stack || error) : message);
}

export function notifyValidationError(message) {
  if (_toast) {
    _toast.add({ severity: 'error', summary: message, life: 50000 });
  }
}
