import { Routes } from '@angular/router';
import { MetroStationsComponent } from './metro-stations/metro-stations.component';
import { HomeComponent } from './home/home.component';

export const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
  },
  {
    path: 'metro-stations',
    component: MetroStationsComponent,
  }
];
