import { Component } from '@angular/core';
import { Routes } from '@angular/router';
import { EmployeesComponets } from './components/employees.componets/employees.componets';

export const routes: Routes = [
  { path: '', redirectTo: 'employees', pathMatch: 'full' },
  { path: 'employees', component: EmployeesComponets },
  { path: '**', redirectTo: 'employees' },
];
