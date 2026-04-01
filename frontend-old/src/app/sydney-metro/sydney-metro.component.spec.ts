import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SydneyMetroComponent } from './sydney-metro.component';

describe('MetroComponent', () => {
  let component: SydneyMetroComponent;
  let fixture: ComponentFixture<SydneyMetroComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SydneyMetroComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SydneyMetroComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
