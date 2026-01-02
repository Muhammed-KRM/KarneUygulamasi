import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class MainConfigService {
  constructor(
    private authService: AuthService
  ) { }
  getMainConfig(configurationName: string, isEnums = false): any[] {
    const data = this.authService.getAllConfigurationDatas();
    if(!data)
      return [];

    if(!isEnums)
      return data[configurationName];
    else{

      const enums = data.response.appEnums.filter((x:any) => x.name === configurationName);
      if(!enums || enums.length === 0)
        return [];
      return enums[0].pairs;
    }
  }
}