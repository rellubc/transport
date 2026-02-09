import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MetroStationsComponent } from './metro-stations.component';

describe('MetroStationsComponent', () => {
  let component: MetroStationsComponent;
  let fixture: ComponentFixture<MetroStationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MetroStationsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MetroStationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
