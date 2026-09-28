import { Experience } from './experience';

export interface Employee {
  employeeId: number;
  employeeName: string;
  isActive: boolean;
  joinDate: string;
  imageUrl?: string;
  imageName?: string;
  experiences: Experience[];
}
