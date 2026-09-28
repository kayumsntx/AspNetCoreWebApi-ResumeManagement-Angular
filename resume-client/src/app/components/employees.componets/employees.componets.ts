import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, signal } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Employee } from '../../models/employee';
import { Experience } from '../../models/experience';
import { ExperienceTitle } from '../../models/experience-title';
import { isActive } from '@angular/router';
import { EmployeeServices } from '../../services/employee.services';
import { ExperienceDTO } from '../../models/experience-dto';
import { TreeError } from '@angular/compiler';

@Component({
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  selector: 'app-employees.componets',
  styleUrl: './employees.componets.css',
  templateUrl: './employees.componets.html',
})
export class EmployeesComponets implements OnInit {
  private fb = inject(FormBuilder);
  private employeeService = inject(EmployeeServices);
  private cdr = inject(ChangeDetectorRef);
  employees = signal<Employee[]>([]);
  titles = signal<ExperienceTitle[]>([]);
  selectedFile = signal<File | null>(null);
  imagePreview = signal<string | null>(null);
  isEditMode = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  editEmployeeId = signal<number | null>(null);
  employeeForm!: FormGroup;
  serverUrl = 'http://localhost:5272';
  Experiences: any;

  ngOnInit(): void {
    this.intForm();
    this.loadEmployees();
    this.loadTitles();
    this.cdr.detectChanges();
  }
  loadTitles(): void {
    this.employeeService.getExpTitle().subscribe({
      next: (data) => this.titles.set(data),
      error: (err) => console.error(err),
    });
  }
  get experiences(): FormArray {
    return this.employeeForm.get('experiences') as FormArray;
  }
  loadEmployees(): void {
    this.employeeService.getEmployees().subscribe({
      next: (data) => {
        this.employees.set(data);
      },
      error: (err) => console.error(err),
    });
  }
  intForm() {
    this.employeeForm = this.fb.group({
      employeeName: ['', [Validators.required]],
      isActive: [true],
      joinDate: ['', [Validators.required]],
      experiences: this.fb.array([]),
    });
  }

  deleteEmployee(id: number): void {
    if (confirm('Are you sure to delete this employee')) {
      this.employeeService.deleteEmployee(id).subscribe({
        next: () => {
          this.loadEmployees();
        },
        error: (err) => {
          console.error(err);
        },
      });
    }
  }
  editEmployee(employee: Employee): void {
    this.isEditMode.set(true);
    this.editEmployeeId.set(employee.employeeId);
    this.selectedFile.set(null);
    this.imagePreview.set(employee.imageUrl ? `${this.serverUrl}${employee.imageUrl}` : null);
    // const joinDate=new Date(employee.joinDate).toISOString().split('T')[0];

    let joinDateFormatted = '';
    if (employee.joinDate) {
      const d = new Date(employee.joinDate);
      if (!isNaN(d.getTime())) {
        joinDateFormatted = d.toISOString().split('T')[0];
      }
    }

    this.employeeForm.patchValue({
      employeeName: employee.employeeName,
      isActive: employee.isActive,
      joinDate: joinDateFormatted,
    });
    this.experiences.clear();
    if (employee.experiences?.length) {
      employee.experiences.forEach((exp) => {
        this.addExperience(exp.experienceTitleId, exp.duration);
      });
    }
  }
  onSubmit(): void {
    if (this.employeeForm.invalid) {
      this.employeeForm.markAllAsTouched();
      return;
    }
    const formValue = this.employeeForm.value;
    const experiencesData: ExperienceDTO[] = formValue.experiences;
    const editId = this.editEmployeeId();
    if (this.isEditMode() && editId !== null) {
      this.employeeService
        .updateEmployee(editId, formValue, this.selectedFile(), experiencesData)
        .subscribe({
          next: () => {
            this.loadEmployees();
            this.resetForm();
          },
          error: (err) => {
            console.error('error in Updateing employee:' + err);
          },
        });
    } else {
      this.employeeService.saveEmployee(formValue, this.selectedFile(), experiencesData).subscribe({
        next: () => {
          this.loadEmployees();
          this.resetForm();
        },
        error: (err) => {
          console.error('Error in saving employee:' + err);
        },
      });
    }
  }
  resetForm() {
    this.isEditMode.set(false);
    this.selectedFile.set(null);
    this.imagePreview.set(null);
    this.employeeForm.reset({ isActive: true });
    this.experiences.clear();
  }
  removeExprience(index: number): void {
    this.experiences.removeAt(index);
  }
  newExperience(titleId: number = 0, duration: number = 0): FormGroup {
    return this.fb.group({
      experienceTitleId: [titleId, [Validators.required, Validators.min(1)]],
      duration: [duration, [Validators.required, Validators.min(1)]],
    });
  }
  addExperience(titleId: number = 0, duration: number = 0): void {
    this.experiences.push(this.newExperience(titleId, duration));
  }
  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.selectedFile.set(file);
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview.set(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  }
}
