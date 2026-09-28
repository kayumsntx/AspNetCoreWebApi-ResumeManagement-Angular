import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { Employee } from '../models/employee';
import { ExperienceTitle } from '../models/experience-title';
import { ExperienceDTO } from '../models/experience-dto';
import { Title } from '@angular/platform-browser';

@Service()
export class EmployeeServices {
  private apiUrl = 'http://localhost:5272/api/Employees';
  private http = inject(HttpClient);
  getEmployees(): Observable<Employee[]> {
    // this.getEmployees():Observable<Employee[]>(this.apiUrl);
    return this.http.get<Employee[]>(this.apiUrl);
  }
  getEmployeeById(id: number): Observable<Employee> {
    return this.http.get<Employee>(`${this.apiUrl}/${id}`);
  }

  getExpTitle(): Observable<ExperienceTitle[]> {
    return this.http.get<ExperienceTitle[]>(`${this.apiUrl}/titles`);
  }
  saveEmployee(
    employeeData: any,
    imageFile: File | null,
    experiences: ExperienceDTO[],
  ): Observable<Employee> {
    const formdata = this.buildFormData(employeeData, imageFile, experiences);
    return this.http.post<Employee>(this.apiUrl, formdata);
  }
  updateEmployee(
    id: number,
    employeeData: any,
    imageFile: File | null,
    experiences: ExperienceDTO[],
  ): Observable<void> {
    const formdata = this.buildFormData(employeeData, imageFile, experiences);
    formdata.append('EmployeeId', id.toString());
    return this.http.put<void>(`${this.apiUrl}/${id}`, formdata);
  }
  deleteEmployee(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private buildFormData(data: any, imageFile: File | null, experiences: ExperienceDTO[]): FormData {
    const formdata = new FormData();
    formdata.append('EmployeeName', data.employeeName);
    formdata.append('isActive', data.isActive ? 'true' : 'false');
    const rawDate = data.joinDate ?? data.joinDate;
    let joinDateFormated = '';
    if (rawDate) {
      const parsedDate = new Date(rawDate);
      if (!isNaN(parsedDate.getTime())) {
        joinDateFormated = parsedDate.toISOString().split('T')[0];
      }
    }

    formdata.append('joinDate', joinDateFormated);

    if (imageFile) {
      formdata.append('imageFile', imageFile);
    } else if (data.imageUrl) {
      formdata.append('ImageUrl', data.imageUrl);
    }
    formdata.append('EXperiencesString', JSON.stringify(experiences));
    return formdata;
  }
}
