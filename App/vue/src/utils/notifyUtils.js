import $ from "jquery";
import notify from "devextreme/ui/notify";

export function notifyError(message, error) {
  // noinspection JSUnusedGlobalSymbols
  let notifyToastOptions = {
    type: 'error',
    displayTime: 5000,
    contentTemplate(element) {
      const $rootDiv = $('<div>')

      const $text = $('<div>').text(message);
      $rootDiv.append($text);

      const $copyLink = $('<a>')
        .attr('href', '#')
        .attr('onclick', `navigator.clipboard.writeText('${message}');`)
        .attr('style', 'color: #99ddff;')
        .text('Копировать текст ошибки');
      $rootDiv.append($copyLink);

      element.append($rootDiv);
    }
  };
  // noinspection JSCheckFunctionSignatures
  notify(notifyToastOptions, { position: "bottom", direction: "up-push" });
  console.error(error != null ? message + '\n' + error.stack : message);
}
