import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SydneyTrainsComponent } from './sydney-trains.component';

describe('SydneyTrainsComponent', () => {
  let component: SydneyTrainsComponent;
  let fixture: ComponentFixture<SydneyTrainsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SydneyTrainsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SydneyTrainsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
