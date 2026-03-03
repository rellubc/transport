import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MetroComponent } from './metro.component';

describe('MetroComponent', () => {
  let component: MetroComponent;
  let fixture: ComponentFixture<MetroComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MetroComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MetroComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
