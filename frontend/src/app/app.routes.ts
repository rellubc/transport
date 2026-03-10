import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { SydneyMetroComponent } from './metro/sydney-metro.component';
import { MapComponent } from './components/map/map.component';
import { SydneyTrainsComponent } from './sydney-trains/sydney-trains.component';
import { SydneyComponent } from './sydney/sydney.component';

export const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
  },
  {
    path: 'sydney',
    component: SydneyComponent,
  },
  {
    path: 'sydney/metro',
    component: SydneyMetroComponent,
  },
  {
    path: 'sydney/trains',
    component: SydneyTrainsComponent,
  },
  {
    path: 'map-test',
    component: MapComponent,
  }
];
