import { ExperienceTitle } from './experience-title';

export interface Experience {
  experienceId: number;
  employeeId: number;
  experienceTitleId: number;
  duration: number;
  experienceTitle: ExperienceTitle;
}
