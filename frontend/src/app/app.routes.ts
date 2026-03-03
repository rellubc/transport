import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MetroComponent } from './metro/metro.component';
import { MapComponent } from './components/map/map.component';

export const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
  },
  {
    path: 'metro',
    component: MetroComponent,
  },
  {
    path: 'map-test',
    component: MapComponent,
  }
];
