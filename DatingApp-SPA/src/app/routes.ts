import { MemberDetailResolver } from './_resolvers/member-detail.resolver';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { AuthGuard } from './_guards/auth.guard';
import { ListsComponent } from './lists/lists.component';
import { MessagesComponent } from './messages/messages.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { HomeComponent } from './home/home.component';
import { Routes } from '@angular/router';
import { MemberListResolver } from './_resolvers/member-list.resolver';

export const appRoutes: Routes = [
    { path: '',     component: HomeComponent },
    {
        // dummy routes, in pratica viene composto il percorso es. http://localhost:4200/[pathvalue]/members
        // dato che path vale '' il percorso generato è http://localhost:4200/members
        // in questo modo vengono raggruppate le 3 routes sottostanti e messe sotto la routes guard [AuthGuard] che
        // assicura che l'utente sia loggato
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [AuthGuard],
        children: [
            { path: 'members',  component: MemberListComponent, resolve: {users: MemberListResolver} },
            { path: 'members/:id',  component: MemberDetailComponent, resolve: {user: MemberDetailResolver} },
            { path: 'messages', component: MessagesComponent },
            { path: 'lists',    component: ListsComponent }
        ]
    },

    { path: '**', redirectTo: '', pathMatch: 'full' }
];