import { Pipe, PipeTransform } from '@angular/core';
import { MainConfigService } from '../services/public/main-config.service';

@Pipe({
  name: 'mainconfig'
})
export class MainConfigPipe implements PipeTransform {
  constructor(private mainConfigService:MainConfigService){}
  transform(value: any, listName:string, valueName:string, columnName:string, isEnums = false): any {
    if(!value)
      return;
    let list:any[] = this.mainConfigService.getMainConfig(listName, isEnums);
    if(list == undefined)
      return "";
    let item = list.filter(x => x[valueName] == value)[0][columnName];
    if(!item)
      return "";
    return item;
  }
}