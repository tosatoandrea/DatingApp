import { Injectable } from '@angular/core';

/*
l'oggetto alerify è dichiarato a livello globale perchè è stato importato nel file angular.json
per usarlo in questo servizio occorre però dichiarare una variabile con lo stesso nome
il fatto di dichiarare la variabile di tipo any serve solo per evitare un warning a livello di tslint
*/
declare let alertify: any;

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {
  constructor() { }

  confirm(message: string, okCallBack: () => any) {
    alertify.confirm(message, function(e) {
      if (e) {
        okCallBack();
      } else {}
    });
  }

  success(message: string) {
    alertify.success(message);
  }

  error(message: string) {
    alertify.error(message);
  }

  warning(message: string) {
    alertify.warning(message);
  }

  message(message: string) {
    alertify.message(message);
  }


}
