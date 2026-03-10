import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SydneyComponent } from './sydney.component';

describe('SydneyComponent', () => {
  let component: SydneyComponent;
  let fixture: ComponentFixture<SydneyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SydneyComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SydneyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
